# Skeletron
Многофункциональный Discord бот, разработанный для внутренних нужд Discord сервера WAV.

> _В связи с узкой специализацией данного бота в репозитории имеется **очень** много хардкода на id каналов, ролей и других сущностей, к которым можно получить доступ только в пределах сервера. Поэтому вероятность успешного self-хоста бота без как минимум консультации с автором репозитория крайне мала._

Большинство функциональных возможностей на данный момент недоступно в связи со значительно снизившейся активностью на сервере и отключенной БД. 
Префикс: `sk!`

# Описание функционала:
## Администрирование сервера
Доступные кооманды для администрирования:
+ `ban [discordMember] [reason]`: забанить указанного ползователя.
  + `discordMember` - пользователь, которого необходимо забанить;
  + `reason` - причина.
+ `mute [discordMember] [reason]`: замьютить указанного пользователя.
  + `discordMember` - пользователь, которого необходимо замьютить;
  + `reason` - причина. 
+ `d [msg] [reason]`: удалить сообщение и уведомить автора об этом;
  + `msg`: ссылка на удаляемое сообщение;
  + `reason`: причина.
+ `rd [targetChannel] [reason]`: переслать сообщение в другой канал и удалить его с предыдущего.
  + `targetChannel`: текстовый канал, куда необходимо переслать сообщение;
  + `reason`: причина.
+ `r [targetChannel]`: переслать сообщение в другой канал.
  + `targetChannel`: текстовый канал, куда необходимо перенаправить сообщение

## osu! команды
Skeletron позволяет привязать свой Discord профиль к osu! профилю. 
Доступные сервера:
+ [Bancho](https://osu.ppy.sh)
+ [Gatari](https://osu.gatari.pw)

### Команды:
+ `osu [nickname]`: получить информацию об osu! профиле.
  + 'nickname': osu! никнейм.
+ `osuset [nickname] [server]`: привязать свой osu! профиль к Discord аккаунту. Привязка работает только в пределах сервера.
  + `nickname`: osu! никнейм;
  + `args`: доступные параметры: 
    + `-gatari`: использовать информацию с сервера gatari

### Распознавание ссылок
Бот автоматически распознает ссылки на домены `osu.ppy.sh`, `gatari.pw` и выводит подробную информацию об объекте.
Поддерживаются ссылки на:
- Карты (bancho, gatari)
- Профили (bancho)

![osu url demo](https://github.com/Flexlug/Skeletron/raw/master/url_demo.gif)

### Распознавание скриншотов
Бот может автоматически загружать скриншоты из игры, распознавать написанный на нём текст. За счет этого бот может сам по скрину найти карту (OCR библиотека: [tesseract-ocr](https://github.com/charlesw/tesseract)).

![osu recog demo](https://github.com/Flexlug/Skeletron/raw/master/recog_demo.gif)

### Проведение конкурса
> _Собственно зачем этот бот и был в своё время написан. На данный момент этот функционал полностью сломан и требует серьезной доработки._

#### Алгоритм проведения конкурса:
1. Участники сервера регистрируются на конкурс через бота. Для этого необходимо:
+ Привязать свой osu! профиль к Discord серверу;
+ Сгенерировать конкурсный профиль.
На основе общедоступной статистики, которая отображалась в профиле osu!, конкурсанту присваивалась соответствующая конкурсная категория. Всего категорий было 6: `beginner`, `alpha`, `beta`, `gamma`, `delta`, `epsilon`.

2. Объявляется набор выбор карт, которые будут на конкурсе.
Администрация сервера заранее вносит предлагаемые карты через команды администратора. Далее через slash-команды конкурсанты могут отдать голос какой-то одной карте, которую они хотели бы видеть на конкурсе. Также и конкурсантов имеется возможность самим предложить **одну** карту. Карты, которые не попали на конкурс, остаются до следующего конкурса. 
Таким образом, в идеале, администрация сервера один раз вносит предлагаемые карты, а далее карты уже выбираются самим коммьюнити. 

3. Начинается конкурс.
В течение отведенного времени конкурсанты должны самостоятельно пройти карту и отправить файл со реплеем в личные сообщения бота через команду `submit`.
Бот автоматически проверяет следующие параметры:
+ Скор был поставлен игроком, который был авторизован в свой профиль;
+ Скор имеет online-id (т.е. скор был субмитнут);
+ Игрок сыграл карту, которая положена его категории;
+ Карта была сыграна именно во время проведения конкурса - не раньше и не позже.

Таким образом формируется лидерборд, который отображается в announce-канале.
По завершении конкурса прием реплеев закрывается. Объявляются победители.

## Другие команды

### Автоматическое распознавание ссылок из VK
Бот может автоматически распознавать ссылки на посты из соц.сети VKontakte и парсить их. 
Поддержкивается:


![vk_demo](https://raw.githubusercontent.com/Flexlug/Skeletron/master/vk_demo.gif)
