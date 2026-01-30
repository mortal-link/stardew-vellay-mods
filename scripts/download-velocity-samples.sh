#!/bin/bash
# 下载 Salamander Grand Piano 6层力度采样
# 从 16 层中选取 6 层: v1, v4, v7, v10, v13, v16

set -e

# 配置
REPO_URL="https://github.com/sfzinstruments/SalamanderGrandPiano/raw/master/Samples"
OUTPUT_DIR="../assets/piano-velocity"
TEMP_DIR="./temp_samples"
SAMPLE_DURATION=5  # 每个采样保留的秒数

# 我们选取的 6 个力度层
VELOCITY_LAYERS=(1 4 7 10 13 16)

# Salamander 采样的音符 (小三度间隔: A, C, D#, F#)
# A0 到 C8 的范围
NOTES=(
    "A0" "C1" "D#1" "F#1"
    "A1" "C2" "D#2" "F#2"
    "A2" "C3" "D#3" "F#3"
    "A3" "C4" "D#4" "F#4"
    "A4" "C5" "D#5" "F#5"
    "A5" "C6" "D#6" "F#6"
    "A6" "C7" "D#7" "F#7"
    "A7" "C8"
)

# 创建目录
mkdir -p "$OUTPUT_DIR"
mkdir -p "$TEMP_DIR"

echo "=== Salamander Grand Piano 6层力度采样下载器 ==="
echo "选取的力度层: ${VELOCITY_LAYERS[*]}"
echo "音符数量: ${#NOTES[@]}"
echo "总计下载: $((${#NOTES[@]} * ${#VELOCITY_LAYERS[@]})) 个采样"
echo "每个采样保留: ${SAMPLE_DURATION} 秒"
echo ""

# 检查 ffmpeg
if ! command -v ffmpeg &> /dev/null; then
    echo "错误: 需要安装 ffmpeg"
    echo "macOS: brew install ffmpeg"
    exit 1
fi

# 下载并转换
download_count=0
total_count=$((${#NOTES[@]} * ${#VELOCITY_LAYERS[@]}))

for note in "${NOTES[@]}"; do
    for vel in "${VELOCITY_LAYERS[@]}"; do
        # Salamander 文件名格式: A0v1.flac
        src_file="${note}v${vel}.flac"
        # URL encode # -> %23
        src_file_encoded="${src_file//#/%23}"
        src_url="${REPO_URL}/${src_file_encoded}"

        # 转换力度层编号为我们的编号 (1,4,7,10,13,16 -> 1,2,3,4,5,6)
        case $vel in
            1)  our_vel=1 ;;
            4)  our_vel=2 ;;
            7)  our_vel=3 ;;
            10) our_vel=4 ;;
            13) our_vel=5 ;;
            16) our_vel=6 ;;
        esac

        # 输出文件名格式: A0_v1.wav (我们的力度层编号)
        out_file="${note}_v${our_vel}.wav"
        out_path="${OUTPUT_DIR}/${out_file}"

        download_count=$((download_count + 1))

        # 跳过已存在的文件
        if [ -f "$out_path" ]; then
            echo "[$download_count/$total_count] 跳过 (已存在): $out_file"
            continue
        fi

        echo "[$download_count/$total_count] 下载: $src_file -> $out_file"

        # 下载 FLAC
        temp_flac="${TEMP_DIR}/${src_file}"
        if curl -sL -o "$temp_flac" "$src_url"; then
            # 检查文件是否有效
            if file "$temp_flac" | grep -q "FLAC"; then
                # 转换为 WAV，保留前 N 秒
                if ffmpeg -y -i "$temp_flac" -t "$SAMPLE_DURATION" "$out_path" 2>/dev/null; then
                    rm "$temp_flac"
                else
                    echo "  警告: 转换失败 $src_file"
                    rm -f "$temp_flac"
                fi
            else
                echo "  警告: 下载文件无效 $src_file"
                rm -f "$temp_flac"
            fi
        else
            echo "  警告: 下载失败 $src_file"
        fi
    done
done

# 清理
rm -rf "$TEMP_DIR"

echo ""
echo "=== 完成 ==="
echo "采样保存在: $OUTPUT_DIR"
echo ""

# 统计
file_count=$(ls -1 "$OUTPUT_DIR"/*.wav 2>/dev/null | wc -l)
total_size=$(du -sh "$OUTPUT_DIR" 2>/dev/null | cut -f1)
echo "文件数量: $file_count"
echo "总大小: $total_size"
