<div align="center">

# 📋 ClipboardHistory

**История буфера обмена для Windows: всё, что ты копировал, под рукой**

![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6?logo=windows&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET%20Framework%204-512BD4?logo=dotnet&logoColor=white)
![No install](https://img.shields.io/badge/установка-не%20нужна-success)
![License](https://img.shields.io/badge/license-MIT-blue)

<img src="docs/screenshot.png" alt="Скриншот" width="420">

</div>

## ✨ Возможности

- 📚 Запоминает последние 100 скопированных текстов
- ⌨️ **Ctrl+Shift+V** открывает окно из любой программы
- 🔍 Мгновенный поиск по истории
- ♻️ Повторы не дублируются, а поднимаются наверх
- 📥 Работает в трее: закрытие окна не останавливает запись
- 🔒 История хранится **только в памяти** и нигде не сохраняется на диск

## ⌨️ Управление

| Клавиша | Действие |
|:---:|---|
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>V</kbd> | Открыть историю |
| <kbd>↓</kbd> из поиска | Перейти к списку |
| <kbd>Enter</kbd> / двойной клик | Скопировать запись |
| <kbd>Del</kbd> | Удалить запись |

Выйти из программы можно правой кнопкой по иконке в трее, пункт «Выход».

## 📥 Установка

1. Скачай `ClipboardHistory.exe` из раздела [**Releases**](../../releases/latest)
2. Запусти его, установка не нужна

## 🛠️ Сборка из исходников

Нужен только Windows: компилятор C# входит в состав .NET Framework.

```bat
build.bat
```

## 📄 Лицензия

[MIT](LICENSE)
