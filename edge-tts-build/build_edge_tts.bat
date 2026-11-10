@echo off
rem edge-tts を1つのフォルダにまとめる（Python不要で動く edge-tts.exe を作る）。
rem 使い方: このバッチを実行 → dist\edge-tts フォルダを、配布フォルダの「edge-tts」へ丸ごとコピー。
rem Microsoft側の仕様変更で読み上げが動かなくなったら、pip で edge-tts を最新にしてから再実行する。
cd /d "%~dp0"
python -m venv venv
venv\Scripts\python -m pip install --upgrade edge-tts pyinstaller
venv\Scripts\pyinstaller --noconfirm --clean --onedir --console --name edge-tts --collect-submodules edge_tts edge_tts_entry.py
echo.
echo 完了: dist\edge-tts を配布フォルダの edge-tts へコピーしてください。
pause
