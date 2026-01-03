using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace PianoBlock
{
    public class PianoBlockMenu : IClickableMenu
    {
        private readonly Vector2 blockPosition;
        private readonly PianoBlockData data;
        private readonly Texture2D letterBg;
        private readonly Texture2D chickenTexture;

        private Rectangle contentArea;
        private int scrollOffset = 0;
        private const int MaxVisibleNotes = 5;
        private const int RowHeight = 54;
        private int lastHoveredNote = -1;

        private readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        // 按钮区域
        private Rectangle playButtonRect;
        private Rectangle addButtonRect;
        private Rectangle scrollUpRect;
        private Rectangle scrollDownRect;
        private List<NoteRowButtons> noteRowButtons = new();


        private class NoteRowButtons
        {
            public int NoteIndex;
            public Rectangle Row;
            public Rectangle PitchDown;
            public Rectangle PitchUp;
            public Rectangle OctaveDown;
            public Rectangle OctaveUp;
            public Rectangle DurationDown;
            public Rectangle DurationUp;
            public Rectangle Delete;
        }

        // 可选的延音时长（毫秒），0表示自然衰减
        private readonly int[] DurationOptions = { 100, 250, 500, 1000, 2000, 0 };

        // 信纸背景的位置和大小
        private Rectangle letterRect;
        private const int TitleHeight = 70;

        public PianoBlockMenu(Vector2 position, PianoBlockData blockData)
            : base(
                Game1.uiViewport.Width / 2 - 320,
                Game1.uiViewport.Height / 2 - 260,
                640, 520, true)
        {
            blockPosition = position;
            data = blockData;
            letterBg = Game1.content.Load<Texture2D>("LooseSprites\\letterBg");
            chickenTexture = Game1.content.Load<Texture2D>("Animals\\White Chicken");

            // 信纸在标题下方，更宽
            letterRect = new Rectangle(
                xPositionOnScreen,
                yPositionOnScreen + TitleHeight,
                width,
                height - TitleHeight
            );

            int padding = 28;
            contentArea = new Rectangle(
                letterRect.X + padding,
                letterRect.Y + padding,
                letterRect.Width - padding * 2,
                letterRect.Height - padding * 2
            );

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            noteRowButtons.Clear();

            int startY = contentArea.Y + 28;
            int baseX = contentArea.X;

            for (int i = 0; i < MaxVisibleNotes; i++)
            {
                int noteIndex = i + scrollOffset;
                if (noteIndex >= data.Notes.Count) break;

                int y = startY + i * RowHeight;

                // 箭头按钮：点击区域和绘制区域完全一致
                // 布局: Note(80) | Pitch箭头(60) | Octave箭头(60) | Duration箭头(80) | Delete(40)
                var rowBtns = new NoteRowButtons
                {
                    NoteIndex = noteIndex,
                    Row = new Rectangle(baseX, y, 75, 44),
                    PitchDown = new Rectangle(baseX + 80, y + 8, 28, 28),
                    PitchUp = new Rectangle(baseX + 108, y + 8, 28, 28),
                    OctaveDown = new Rectangle(baseX + 150, y + 8, 28, 28),
                    OctaveUp = new Rectangle(baseX + 178, y + 8, 28, 28),
                    DurationDown = new Rectangle(baseX + 230, y + 8, 28, 28),
                    DurationUp = new Rectangle(baseX + 258, y + 8, 28, 28),
                    Delete = new Rectangle(baseX + 310, y + 6, 40, 36)
                };
                noteRowButtons.Add(rowBtns);
            }

            // 滚动按钮
            scrollUpRect = new Rectangle(contentArea.Right - 50, contentArea.Y + 30, 44, 44);
            scrollDownRect = new Rectangle(contentArea.Right - 50, contentArea.Y + MaxVisibleNotes * RowHeight - 15, 44, 44);

            // 底部按钮
            int bottomY = contentArea.Y + MaxVisibleNotes * RowHeight + 20;
            playButtonRect = new Rectangle(baseX, bottomY, 48, 48);
            addButtonRect = new Rectangle(baseX + 60, bottomY, 48, 48);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            foreach (var row in noteRowButtons)
            {
                if (row.Row.Contains(x, y))
                {
                    if (lastHoveredNote != row.NoteIndex)
                    {
                        lastHoveredNote = row.NoteIndex;
                        PlaySingleNote(data.Notes[row.NoteIndex]);
                    }
                    return;
                }
            }
            lastHoveredNote = -1;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // DEBUG: 显示点击位置
            Game1.addHUDMessage(new HUDMessage($"Click: {x}, {y}", 2));

            // 滚动
            if (data.Notes.Count > MaxVisibleNotes)
            {
                if (scrollUpRect.Contains(x, y) && scrollOffset > 0)
                {
                    scrollOffset--;
                    UpdateLayout();
                    Game1.playSound("shiny4");
                    return;
                }
                if (scrollDownRect.Contains(x, y) && scrollOffset < data.Notes.Count - MaxVisibleNotes)
                {
                    scrollOffset++;
                    UpdateLayout();
                    Game1.playSound("shiny4");
                    return;
                }
            }

            if (playButtonRect.Contains(x, y))
            {
                PlayAllNotes();
                Game1.playSound("drumkit6");
                return;
            }

            if (addButtonRect.Contains(x, y))
            {
                data.Notes.Add(new MusicNote());
                UpdateLayout();
                Game1.playSound("coin");
                return;
            }

            foreach (var row in noteRowButtons)
            {
                if (row.PitchDown.Contains(x, y))
                {
                    data.Notes[row.NoteIndex].Pitch = (data.Notes[row.NoteIndex].Pitch - 1 + 12) % 12;
                    PlaySingleNote(data.Notes[row.NoteIndex]);
                    return;
                }

                if (row.PitchUp.Contains(x, y))
                {
                    data.Notes[row.NoteIndex].Pitch = (data.Notes[row.NoteIndex].Pitch + 1) % 12;
                    PlaySingleNote(data.Notes[row.NoteIndex]);
                    return;
                }

                if (row.OctaveDown.Contains(x, y))
                {
                    data.Notes[row.NoteIndex].Octave = Math.Max(1, data.Notes[row.NoteIndex].Octave - 1);
                    PlaySingleNote(data.Notes[row.NoteIndex]);
                    return;
                }

                if (row.OctaveUp.Contains(x, y))
                {
                    data.Notes[row.NoteIndex].Octave = Math.Min(7, data.Notes[row.NoteIndex].Octave + 1);
                    PlaySingleNote(data.Notes[row.NoteIndex]);
                    return;
                }

                if (row.DurationDown.Contains(x, y))
                {
                    CycleDuration(row.NoteIndex, -1);
                    Game1.playSound("smallSelect");
                    return;
                }

                if (row.DurationUp.Contains(x, y))
                {
                    CycleDuration(row.NoteIndex, 1);
                    Game1.playSound("smallSelect");
                    return;
                }

                if (row.Delete.Contains(x, y) && data.Notes.Count > 1)
                {
                    data.Notes.RemoveAt(row.NoteIndex);
                    if (scrollOffset > 0 && scrollOffset >= data.Notes.Count - MaxVisibleNotes + 1)
                        scrollOffset--;
                    UpdateLayout();
                    Game1.playSound("trashcan");
                    return;
                }

                if (row.Row.Contains(x, y))
                {
                    PlaySingleNote(data.Notes[row.NoteIndex]);
                    return;
                }
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            int maxScroll = Math.Max(0, data.Notes.Count - MaxVisibleNotes);
            if (direction > 0 && scrollOffset > 0)
            {
                scrollOffset--;
                UpdateLayout();
            }
            else if (direction < 0 && scrollOffset < maxScroll)
            {
                scrollOffset++;
                UpdateLayout();
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
        }

        private void PlaySingleNote(MusicNote note)
        {
            try
            {
                ModEntry.PlayNote(note);
            }
            catch
            {
                // Ignore errors
            }
        }

        private void PlayAllNotes()
        {
            foreach (var note in data.Notes)
            {
                PlaySingleNote(note);
            }
        }

        private void CycleDuration(int noteIndex, int direction)
        {
            var note = data.Notes[noteIndex];
            // 找到当前时值在选项中的位置
            int currentIdx = 2; // 默认为500ms的位置
            for (int i = 0; i < DurationOptions.Length; i++)
            {
                if (note.DurationMs == DurationOptions[i])
                {
                    currentIdx = i;
                    break;
                }
            }
            // 循环切换
            int newIdx = (currentIdx + direction + DurationOptions.Length) % DurationOptions.Length;
            note.DurationMs = DurationOptions[newIdx];
        }

        private string GetDurationDisplay(int durationMs)
        {
            // 将时值转换为易读的显示格式
            if (durationMs == 0) return "~";  // 自然衰减
            if (durationMs >= 1000) return $"{durationMs / 1000}s";
            return $"{durationMs}";
        }

        public override void draw(SpriteBatch b)
        {
            // 背景遮罩
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // 标题（在信纸上方）
            SpriteText.drawStringWithScrollCenteredAt(b, "Piano Block", xPositionOnScreen + width / 2, yPositionOnScreen + 5);

            // 信纸背景（第一个精灵，位于0,0，大小320x180）
            Rectangle sourceRect = new Rectangle(0, 0, 320, 180);
            b.Draw(letterBg, letterRect, sourceRect, Color.White);

            // 列标题
            int headerY = contentArea.Y;
            Utility.drawTextWithShadow(b, "Note", Game1.smallFont, new Vector2(contentArea.X + 10, headerY), Game1.textColor);
            Utility.drawTextWithShadow(b, "Pitch", Game1.smallFont, new Vector2(contentArea.X + 80, headerY), Game1.textColor);
            Utility.drawTextWithShadow(b, "Oct", Game1.smallFont, new Vector2(contentArea.X + 152, headerY), Game1.textColor);
            Utility.drawTextWithShadow(b, "Len", Game1.smallFont, new Vector2(contentArea.X + 230, headerY), Game1.textColor);

            DrawNoteList(b);

            // 滚动按钮
            if (data.Notes.Count > MaxVisibleNotes)
            {
                bool canUp = scrollOffset > 0;
                bool canDown = scrollOffset < data.Notes.Count - MaxVisibleNotes;

                // 上箭头
                b.Draw(Game1.mouseCursors, new Vector2(scrollUpRect.X + 10, scrollUpRect.Y + 12),
                    new Rectangle(421, 459, 11, 12), canUp ? Color.White : Color.White * 0.35f, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);

                // 下箭头
                b.Draw(Game1.mouseCursors, new Vector2(scrollDownRect.X + 10, scrollDownRect.Y + 10),
                    new Rectangle(421, 472, 11, 12), canDown ? Color.White : Color.White * 0.35f, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);

                string scrollText = $"{scrollOffset + 1}-{Math.Min(scrollOffset + MaxVisibleNotes, data.Notes.Count)}/{data.Notes.Count}";
                Vector2 scrollSize = Game1.tinyFont.MeasureString(scrollText);
                Utility.drawTextWithShadow(b, scrollText, Game1.tinyFont,
                    new Vector2(scrollUpRect.X + 22 - scrollSize.X / 2, scrollUpRect.Bottom + 15), Color.Gray);
            }

            DrawBottomControls(b);

            // 关闭按钮
            base.draw(b);
            drawMouse(b);
        }

        private void DrawNoteList(SpriteBatch b)
        {
            foreach (var row in noteRowButtons)
            {
                var note = data.Notes[row.NoteIndex];

                if (lastHoveredNote == row.NoteIndex)
                {
                    b.Draw(Game1.staminaRect, new Rectangle(contentArea.X - 4, row.Row.Y - 2, 380, 48), Color.Gold * 0.2f);
                }

                string noteText = $"{row.NoteIndex + 1}. {NoteNames[note.Pitch]}{note.Octave}";
                Utility.drawTextWithShadow(b, noteText, Game1.dialogueFont,
                    new Vector2(row.Row.X + 2, row.Row.Y + 6), Game1.textColor);

                // DEBUG: 绘制点击区域边框 (更明显的颜色)
                b.Draw(Game1.staminaRect, row.PitchDown, Color.Red * 0.5f);
                b.Draw(Game1.staminaRect, row.PitchUp, Color.Lime * 0.5f);
                b.Draw(Game1.staminaRect, row.OctaveDown, Color.Blue * 0.5f);
                b.Draw(Game1.staminaRect, row.OctaveUp, Color.Yellow * 0.5f);
                b.Draw(Game1.staminaRect, row.DurationDown, Color.Cyan * 0.5f);
                b.Draw(Game1.staminaRect, row.DurationUp, Color.Magenta * 0.5f);
                b.Draw(Game1.staminaRect, row.Delete, Color.Red * 0.5f);

                // 箭头绘制在点击区域正中央 (箭头原始11x12，缩放2倍=22x24)
                // Pitch 下箭头
                b.Draw(Game1.mouseCursors, new Vector2(row.PitchDown.X + 3, row.PitchDown.Y + 2),
                    new Rectangle(421, 472, 11, 12), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
                // Pitch 上箭头
                b.Draw(Game1.mouseCursors, new Vector2(row.PitchUp.X + 3, row.PitchUp.Y + 2),
                    new Rectangle(421, 459, 11, 12), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
                // Octave 下箭头
                b.Draw(Game1.mouseCursors, new Vector2(row.OctaveDown.X + 3, row.OctaveDown.Y + 2),
                    new Rectangle(421, 472, 11, 12), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
                // Octave 上箭头
                b.Draw(Game1.mouseCursors, new Vector2(row.OctaveUp.X + 3, row.OctaveUp.Y + 2),
                    new Rectangle(421, 459, 11, 12), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
                // Duration 下箭头
                b.Draw(Game1.mouseCursors, new Vector2(row.DurationDown.X + 3, row.DurationDown.Y + 2),
                    new Rectangle(421, 472, 11, 12), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
                // Duration 上箭头
                b.Draw(Game1.mouseCursors, new Vector2(row.DurationUp.X + 3, row.DurationUp.Y + 2),
                    new Rectangle(421, 459, 11, 12), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);

                // 显示当前时值（在Duration箭头右侧）
                string durText = GetDurationDisplay(note.DurationMs);
                Utility.drawTextWithShadow(b, durText, Game1.smallFont,
                    new Vector2(row.DurationUp.Right + 4, row.DurationUp.Y + 4), Color.SaddleBrown);

                if (data.Notes.Count > 1)
                {
                    // 红色叉号文字
                    Utility.drawTextWithShadow(b, "X", Game1.dialogueFont,
                        new Vector2(row.Delete.X + 10, row.Delete.Y + 2), Color.DarkRed);
                }

                // 分隔线
                b.Draw(Game1.staminaRect, new Rectangle(contentArea.X, row.Row.Bottom + 3, 380, 1), Color.Gray * 0.3f);
            }
        }

        private void DrawBottomControls(SpriteBatch b)
        {
            // DEBUG: 绘制底部按钮点击区域
            b.Draw(Game1.staminaRect, playButtonRect, Color.Cyan * 0.3f);
            b.Draw(Game1.staminaRect, addButtonRect, Color.Magenta * 0.3f);

            // Play 按钮 - 白色小鸡图标
            b.Draw(chickenTexture, new Rectangle(playButtonRect.X, playButtonRect.Y, 48, 48),
                new Rectangle(0, 0, 16, 16), Color.White);

            // Add 按钮 - 绿色加号
            b.Draw(Game1.mouseCursors, new Vector2(addButtonRect.X + 8, addButtonRect.Y + 8),
                new Rectangle(0, 410, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
        }

        private void DrawSmallButton(SpriteBatch b, Rectangle rect, string text, bool isDelete = false)
        {
            Color tint = isDelete ? new Color(255, 180, 180) : Color.White;

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9),
                rect.X, rect.Y, rect.Width, rect.Height, tint, 4f, false);

            Vector2 textSize = Game1.smallFont.MeasureString(text);
            Vector2 textPos = new Vector2(
                rect.X + (rect.Width - textSize.X) / 2,
                rect.Y + (rect.Height - textSize.Y) / 2
            );
            Color textColor = isDelete ? new Color(139, 69, 69) : Game1.textColor;
            Utility.drawTextWithShadow(b, text, Game1.smallFont, textPos, textColor);
        }
    }
}
