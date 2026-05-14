# музыкальный рекомендательный сервис
Курсовой проект по разработке БД и приложения, реализующего функции рекомендательного музыкального сервиса.

## стек технологий:
- ASP.NET Core 9 (Razor Pages)
- PostgreSQL
- Entity Framework Core 9
- Bootstrap 5

## основные функции:
- Регистрация и авторизация (с использованием статического адаптера сессий).
- Поиск треков по названию, артисту и альбому.
- Система оценки (лайк/дизлайк).
- Автоматическая генерация рекомендаций на уровне СУБД.
- Управление плейлистами.

## установка зависимостей
Для корректной работы проекта и взаимодействия с базой данных необходимо установить следующие NuGet-пакеты (через Package Manager Console):

Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design -Version 9.0.0
Install-Package Microsoft.EntityFrameworkCore -Version 9.0.0
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 9.0.0
Install-Package Microsoft.EntityFrameworkCore.Design -Version 9.0.0
Install-Package Microsoft.EntityFrameworkCore.Relational -Version 9.0.0
Install-Package Npgsql -Version 9.0.2
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL -Version 9.0.2

## как запустить:
1. Выполнить SQL-скрипт из папки `/Database/Script.sql`.
2. Указать свою строку подключения в `Program.cs`.
3. Запустить проект через Visual Studio.