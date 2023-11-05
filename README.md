# Skeletron
Многофункциональный Discord бот, изначально разработанный для внутренних нужд Discord сервера WAV.

Префикс: `sk!`

# Запуск и деплой

## Требования:

- Docker;
- Созданный Discord Bot с выданными Intents;
- Standalone приложение в VK;
- OAuth2 приложение на Bancho;

## Инструкция:

1. [Создайте](https://discord.com/developers/applications/) Discord бота. Скопируйте **client secret** во вкладке OAuth2.
2. Во вкладке [Bot](https://discord.com/developers/applications/750768015842345050/bot) выдайте боту **Privileged Gateway Intents**. В частности **presence intent**, **server members intent**, **message content intent**;
3. [Cоздайте](https://vk.com/apps?act=manage) Standalone приложение в VK. Включать его необязательно. В настройках приложения скопируйте **сервисный ключ**;
4. [Создайте](https://osu.ppy.sh/home/account/edit) OAuth2 приложение на сайте Bancho. Скопируйте **ID приложения** и **ключ приложения**;
5. Склонируйте репозиторий. Создайте файл `docker-compose.yml` в корне проекта. Скопируйте в него содержимое `docker-compose-sample.yml` (уже есть в репозитории). Заполните пустые поля:
```yml
version: '3.8'

services:
  skeletron:
    image: flexlug/skeletron:latest
    build:
      context: .
    restart: unless-stopped
    environment:
      # Discord authorization
      "Token": "DISCORD_SECRET"
      
      # For osu services
      "BanchoClientId": 123456
      "BanchoSecret": "BANCHO_SECRET"
      
      # Due to all info from VK links is being retrieved from official VK API
      # you have to specify VK Standalone Application Token
      "VkSecret": "VK_SECRET"
```
6. Запустите бота
```bash
docker compose up -d
```

# Описание функционала:
## Администрирование сервера
Доступные кооманды для администрирования:
+ `d [msg] [reason]`: удалить сообщение и уведомить автора об этом;
  + `msg`: ссылка на удаляемое сообщение;
  + `reason`: причина.
+ `rd [targetChannel] [reason]`: переслать сообщение в другой канал и удалить его с предыдущего.
  + `targetChannel`: текстовый канал, куда необходимо переслать сообщение;
  + `reason`: причина.
+ `r [targetChannel]`: переслать сообщение в другой канал.
  + `targetChannel`: текстовый канал, куда необходимо перенаправить сообщение

## Распознавание ссылок
Бот автоматически распознает ссылки на домены `osu.ppy.sh`, `gatari.pw` и выводит подробную информацию об объекте.
Поддерживаются ссылки на:
- Карты (bancho, gatari)
- Профили (bancho)

![osu url demo](https://raw.githubusercontent.com/Flexlug/Skeletron/master/docs/url_demo.gif)

## Автоматическое распознавание ссылок из VK
Бот может автоматически распознавать ссылки на посты из соц.сети VKontakte и парсить их. 
Поддержкивается:


![vk_demo](https://raw.githubusercontent.com/Flexlug/Skeletron/master/docs/vk_demo.gif)
