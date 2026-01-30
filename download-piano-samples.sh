#!/bin/bash

# 下载 tonejs-instruments 钢琴采样并重命名
# 来源: https://github.com/nbrosowsky/tonejs-instruments/tree/master/samples/piano

OUTPUT_DIR="assets/audio"
BASE_URL="https://raw.githubusercontent.com/nbrosowsky/tonejs-instruments/master/samples/piano"

# 创建输出目录
mkdir -p "$OUTPUT_DIR"

# 定义所有音符 (GitHub上的命名: Cs = C#, Ds = D#, etc.)
NOTES=("A" "As" "B" "C" "Cs" "D" "Ds" "E" "F" "Fs" "G" "Gs")

# 定义八度范围 (钢琴通常是 A0 到 C8)
# 根据 tonejs-instruments 的实际文件

echo "开始下载钢琴采样..."
echo "输出目录: $OUTPUT_DIR"
echo ""

count=0
failed=0

# 下载函数
download_note() {
    local note=$1
    local octave=$2
    local filename="${note}${octave}.wav"
    local url="${BASE_URL}/${filename}"

    # 转换命名: Cs -> C#, Ds -> D#, etc.
    local new_note=$note
    if [[ "$note" == *"s" ]]; then
        new_note="${note%s}#"
    fi
    local new_filename="piano_${new_note}${octave}.wav"

    # 下载文件
    if curl -s -f -o "$OUTPUT_DIR/$new_filename" "$url"; then
        echo "✓ 下载成功: $new_filename"
        return 0
    else
        return 1
    fi
}

# 遍历所有可能的音符组合
for octave in 0 1 2 3 4 5 6 7 8; do
    for note in "${NOTES[@]}"; do
        if download_note "$note" "$octave"; then
            ((count++))
        fi
    done
done

echo ""
echo "================================"
echo "下载完成!"
echo "成功下载: $count 个文件"
echo "保存位置: $OUTPUT_DIR"
echo "================================"

# 列出下载的文件
echo ""
echo "已下载的文件:"
ls -la "$OUTPUT_DIR"/*.wav 2>/dev/null | wc -l
echo "个 WAV 文件"
