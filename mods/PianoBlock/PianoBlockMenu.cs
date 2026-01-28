using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace PianoBlock
{
    /// <summary>钢琴块配置菜单</summary>
    public class PianoBlockMenu : IClickableMenu
    {
        private readonly Vector2 blockPosition;
        private readonly PianoBlockData data;

        // UI组件
        private readonly List<ClickableComponent> noteComponents = new();
        private ClickableTextureComponent? playButton;
        private ClickableTextureComponent? addNoteButton;
        private ClickableTextureComponent? okButton;
        private ClickableTextureComponent? tempoUpButton;
        private ClickableTextureComponent? tempoDownButton;

        private int selectedNoteIndex = 0;
        private int scrollOffset = 0;
        private const int maxVisibleNotes = 8;

        private string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public PianoBlockMenu(Vector2 position, PianoBlockData blockData)
            : base(Game1.uiViewport.Width / 2 - 400, Game1.uiViewport.Height / 2 - 300, 800, 600)
        {
            blockPosition = position;
            data = blockData;

            SetupComponents();
        }

        private void SetupComponents()
        {
            int buttonSize = 64;
            int centerX = xPositionOnScreen + width / 2;
            int bottomY = yPositionOnScreen + height - 80;

            // 播放按钮
            playButton = new ClickableTextureComponent(
                new Rectangle(centerX - 200, bottomY, buttonSize, buttonSize),
                Game1.mouseCursors,
                new Rectangle(128, 256, 64, 64),
                1f);

            // 添加音符按钮
            addNoteButton = new ClickableTextureComponent(
                new Rectangle(centerX - 100, bottomY, buttonSize, buttonSize),
                Game1.mouseCursors,
                new Rectangle(0, 428, 10, 10),
                4f);

            // OK按钮
            okButton = new ClickableTextureComponent(
                "OK",
                new Rectangle(centerX + 100, bottomY, buttonSize, buttonSize),
                null,
                "OK",
                Game1.mouseCursors,
                new Rectangle(128, 256, 64, 64),
                1f);

            // 曲速按钮
            tempoUpButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 120, yPositionOnScreen + 100, 44, 48),
                Game1.mouseCursors,
                new Rectangle(421, 459, 11, 12),
                4f);

            tempoDownButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 120, yPositionOnScreen + 160, 44, 48),
                Game1.mouseCursors,
                new Rectangle(421, 472, 11, 12),
                4f);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // 播放按钮
            if (playButton?.containsPoint(x, y) ?? false)
            {
                PlaySequence();
                Game1.playSound("drumkit6");
            }

            // 添加音符按钮
            if (addNoteButton?.containsPoint(x, y) ?? false)
            {
                data.Notes.Add(new MusicNote());
                Game1.playSound("coin");
            }

            // OK按钮
            if (okButton?.containsPoint(x, y) ?? false)
            {
                exitThisMenu();
                Game1.playSound("bigDeSelect");
            }

            // 曲速调整
            if (tempoUpButton?.containsPoint(x, y) ?? false)
            {
                data.Tempo = Math.Max(100, data.Tempo - 50);
                Game1.playSound("smallSelect");
            }

            if (tempoDownButton?.containsPoint(x, y) ?? false)
            {
                data.Tempo = Math.Min(2000, data.Tempo + 50);
                Game1.playSound("smallSelect");
            }

            // 检查是否点击了音符列表
            CheckNoteListClick(x, y);
        }

        private void CheckNoteListClick(int x, int y)
        {
            int listX = xPositionOnScreen + 50;
            int listY = yPositionOnScreen + 100;
            int itemHeight = 60;

            for (int i = 0; i < Math.Min(data.Notes.Count, maxVisibleNotes); i++)
            {
                int index = i + scrollOffset;
                if (index >= data.Notes.Count) break;

                Rectangle noteRect = new Rectangle(listX, listY + i * itemHeight, 300, 50);

                if (noteRect.Contains(x, y))
                {
                    selectedNoteIndex = index;
                    Game1.playSound("smallSelect");
                    return;
                }

                // 删除按钮
                Rectangle deleteRect = new Rectangle(listX + 310, listY + i * itemHeight + 10, 32, 32);
                if (deleteRect.Contains(x, y) && data.Notes.Count > 1)
                {
                    data.Notes.RemoveAt(index);
                    if (selectedNoteIndex >= data.Notes.Count)
                        selectedNoteIndex = data.Notes.Count - 1;
                    Game1.playSound("trashcan");
                    return;
                }

                // 音高调整按钮
                Rectangle pitchUpRect = new Rectangle(listX + 350, listY + i * itemHeight, 30, 24);
                Rectangle pitchDownRect = new Rectangle(listX + 350, listY + i * itemHeight + 26, 30, 24);

                if (pitchUpRect.Contains(x, y))
                {
                    data.Notes[index].Pitch = (data.Notes[index].Pitch + 1) % 12;
                    Game1.playSound("toolSwap");
                    return;
                }

                if (pitchDownRect.Contains(x, y))
                {
                    data.Notes[index].Pitch = (data.Notes[index].Pitch - 1 + 12) % 12;
                    Game1.playSound("toolSwap");
                    return;
                }

                // 八度调整按钮
                Rectangle octaveUpRect = new Rectangle(listX + 390, listY + i * itemHeight, 30, 24);
                Rectangle octaveDownRect = new Rectangle(listX + 390, listY + i * itemHeight + 26, 30, 24);

                if (octaveUpRect.Contains(x, y))
                {
                    data.Notes[index].Octave = Math.Min(8, data.Notes[index].Octave + 1);
                    Game1.playSound("toolSwap");
                    return;
                }

                if (octaveDownRect.Contains(x, y))
                {
                    data.Notes[index].Octave = Math.Max(0, data.Notes[index].Octave - 1);
                    Game1.playSound("toolSwap");
                    return;
                }
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            if (direction > 0)
                scrollOffset = Math.Max(0, scrollOffset - 1);
            else
                scrollOffset = Math.Min(Math.Max(0, data.Notes.Count - maxVisibleNotes), scrollOffset + 1);
        }

        private void PlaySequence()
        {
            // 播放音符序列
            for (int i = 0; i < data.Notes.Count; i++)
            {
                var note = data.Notes[i];
                PlayNote(note, i);
            }
        }

        private void PlayNote(MusicNote note, int delay)
        {
            // 使用游戏的定时器系统延迟播放
            Game1.delayedActions.Add(new DelayedAction(delay * data.Tempo / 10, () =>
            {
                // 使用flute音效，通过音高参数调整
                Game1.playSound("flute", note.GetGamePitch());
            }));
        }

        public override void draw(SpriteBatch b)
        {
            // 绘制背景遮罩
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // 绘制主窗口
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

            // 绘制标题
            string title = "钢琴块配置";
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            b.DrawString(Game1.dialogueFont, title,
                new Vector2(xPositionOnScreen + width / 2 - titleSize.X / 2, yPositionOnScreen + 20),
                Game1.textColor);

            // 绘制曲速信息
            string tempoText = $"曲速: {data.Tempo}ms";
            b.DrawString(Game1.smallFont, tempoText,
                new Vector2(xPositionOnScreen + width - 200, yPositionOnScreen + 130),
                Game1.textColor);

            // 绘制音符列表
            DrawNoteList(b);

            // 绘制按钮
            playButton?.draw(b);
            addNoteButton?.draw(b);
            okButton?.draw(b);
            tempoUpButton?.draw(b);
            tempoDownButton?.draw(b);

            // 绘制按钮标签
            DrawButtonLabels(b);

            // 绘制鼠标
            drawMouse(b);
        }

        private void DrawNoteList(SpriteBatch b)
        {
            int listX = xPositionOnScreen + 50;
            int listY = yPositionOnScreen + 100;
            int itemHeight = 60;

            // 绘制列表标题
            b.DrawString(Game1.smallFont, "音符序列:", new Vector2(listX, listY - 30), Game1.textColor);

            for (int i = 0; i < Math.Min(data.Notes.Count, maxVisibleNotes); i++)
            {
                int index = i + scrollOffset;
                if (index >= data.Notes.Count) break;

                var note = data.Notes[index];
                int y = listY + i * itemHeight;

                // 绘制选中背景
                if (index == selectedNoteIndex)
                {
                    b.Draw(Game1.staminaRect, new Rectangle(listX - 5, y - 5, 440, 55), Color.Yellow * 0.3f);
                }

                // 绘制音符信息
                string noteText = $"{index + 1}. {note.GetNoteName()}";
                b.DrawString(Game1.dialogueFont, noteText, new Vector2(listX, y), Game1.textColor);

                // 绘制删除按钮（X）
                if (data.Notes.Count > 1)
                {
                    b.DrawString(Game1.dialogueFont, "X",
                        new Vector2(listX + 310, y), Color.Red);
                }

                // 绘制调整按钮
                b.DrawString(Game1.smallFont, "↑", new Vector2(listX + 355, y - 2), Game1.textColor);
                b.DrawString(Game1.smallFont, "↓", new Vector2(listX + 355, y + 22), Game1.textColor);
                b.DrawString(Game1.smallFont, "8↑", new Vector2(listX + 393, y - 2), Game1.textColor);
                b.DrawString(Game1.smallFont, "8↓", new Vector2(listX + 393, y + 22), Game1.textColor);
            }

            // 绘制滚动指示器
            if (data.Notes.Count > maxVisibleNotes)
            {
                string scrollHint = $"{scrollOffset + 1}-{Math.Min(scrollOffset + maxVisibleNotes, data.Notes.Count)} / {data.Notes.Count}";
                b.DrawString(Game1.smallFont, scrollHint,
                    new Vector2(listX, listY + maxVisibleNotes * itemHeight + 10), Color.Gray);
            }
        }

        private void DrawButtonLabels(SpriteBatch b)
        {
            if (playButton != null)
            {
                string playText = "播放";
                Vector2 textSize = Game1.smallFont.MeasureString(playText);
                b.DrawString(Game1.smallFont, playText,
                    new Vector2(playButton.bounds.Center.X - textSize.X / 2, playButton.bounds.Bottom + 5),
                    Game1.textColor);
            }

            if (addNoteButton != null)
            {
                string addText = "添加";
                Vector2 textSize = Game1.smallFont.MeasureString(addText);
                b.DrawString(Game1.smallFont, addText,
                    new Vector2(addNoteButton.bounds.Center.X - textSize.X / 2, addNoteButton.bounds.Bottom + 5),
                    Game1.textColor);
            }
        }
    }
}
