> [!IMPORTANT]
> В проекте используется **.NET 8**.
> Перед тем как начать разработку, ознакомься с [Wiki](https://github.com/AbroGames/NeonWarfare/wiki).

> [!NOTE]
> Чтобы в Godot настроить интеграцию с Rider, необходимо зайти в Editor -> Editor Settings -> Dotnet -> Editor.  
> В списке External Editor выбрать JetBrains Rider и очистить значение Custom Exec Path Args. 

> [!NOTE]
> Для более быстрого дебага игру можно запускать с различными параметрами командной строки.  
> `--server` сразу запускает игровой сервер.  
> `--auto-connect-ip 127.0.0.1` запускает клиент и автоматически подключается к серверу по адресу `127.0.0.1`  
> `--fast-test` запускает сервер и клиент, клиент автоматически подключается к этому серверу

## Структура проекта
### Основные модули
Проект разделен на несколько модулей, каждый из которых отвечает за отдельную сцену или функциональность.
- **Scenes**: Содержит игровые сцены и их обработчики
    - `Root`: Самая верхнеуровневая сцена. Вызывает инициализцию других систем, содержит в себе сцену Game или MainMenu, а также ссылку на PackedScenesContainer.
    - `Game`: Сцена с игрой (не меню). Отвечает за инициализацию сети, подключение, управление общеигровым состоянием, содержит в себе сцену SafeWorld или BattleWorld.
    - `PackedScenesContainer`: Контейнер, который содержит ссылки на все прототипы сцен в игре (тип PackedScene).
    - `Screen`: Содержит набор сцен с интерфейсом пользователя (HUD, меню, экран загрузки, консоль сервера).
    - `World`: Содержит верхнеуровневые сцены SafeWorld и BattleWorld, а также все игровые объекты и их логику.
- **Scripts**: Содержит вспомогательный код, не содержит сцены (tscn файлы)
    - `Content`: Содержит контейнеры со ссылками на ресурсы игры (музыка, звуки, текстуры, частицы)
    - `KludgeBox`: Библиотека с наработками из других проектов (шина событий, перехват сетевых пакетов, логеры, таймеры и т.п.)
    - `Net`: Код для сетевого взаимодействия.
    - `Utils`: Вспомогательные классы (обработка ошибок, работа с аргументами командной строки) и код вспомогательных нод, без tscn файлов (нода с системой пинга, нода-таймер)

### Объединение сцен и классов
Сцены и обработчики хранятся в одной папке. Например, в папке `ClientRoot` хранится `ClientRoot.tscn` (сцена) и `ClientRoot.cs` (обработчик).  
Обработчики могут быть разбиты на несколько `partial` классов по функционалу. Например, класс `ClientRoot` разбит на файлы `ClientRoot.cs`, `ClientRootSceneContainer.cs`, `ClientRootSingleton.cs`.

### Разделение на клиент и сервер
Сцены и обработчики для клиента и сервера хранятся в соседних папках. Например:
- **Root**: Общая папка со всеми файлами связанными со сценой Root
    - **ClientRoot**: Папка с файлами сцены `Root` для клиента.
        - `ClientRoot.tscn`
        - `ClientRoot.cs`
        - `ClientRootSceneContainer.cs`
        - `ClientRootSingleton.cs`
    - **ServerRoot**: Папка с файлами сцены `Root` для сервера.
        - `ServerRoot.tscn`
        - `ServerRoot.cs`
        - `ServerRootSceneContainer.cs`
        - `ServerRootSingleton.cs`
    - `RootService.cs`: Статичный класс с общими функциями, которые используются и на клиенте и на сервере.

### Поток инициализации
1. `Root/StarterScene`
    - Является точкой входа и определяет, будет ли запускаться `ClientRoot` или `ServerRoot` сцена.
2. `ClientRoot` / `ServerRoot`
    - Вызывает инициализацию других систем в `Init()` и запускает игру в `Start()`.
    - Является контейнером для сцен `MainMenuMainScene` и `ClientGame` / `ServerGame`.
3. `ClientGame` / `ServerGame`
    - Инициализирует сеть и осуществляет подключение.
    - Является контейнером для сцен типа `World`.

### Поток отключения
- Завершение работы клиента
    - Завершение работы клиента осуществляется при помощи вызова функции `ClientRoot#Shutdown()`.
    - При любом способе уничтожения сцены `ClientGame` (включая завершение игры), автоматически будет вызван `ClientGame#ServerShutdowner`. И если данный клиент является хостом (запускал серверный процесс), то будет вызвана функция `OS.Kill()` для завершения работы сервера.
- Завершение работы сервера
    - Завершение работы сервера осуществляется при помощи вызова функции `ServerRoot#Shutdown()`.
    - Дополнительно на сервере каждые несколько секунд запускается проверка в `ServerGame#ClientDeadChecker`. И если в ОС не будет найден процесс клиента игры, который запустил данный сервер, то сервер автоматически завершит свою работу.

## Основные инфраструктурные сцены и их обработчики
### Root
- **Root**: Общая папка со всеми файлами связанными со сценой `Root`
    - **ClientRoot**: Папка с файлами сцены `Root` для клиента.
        - `ClientRoot.tscn`: Техническая сцена без игровых объектов.
        - `ClientRoot.cs`: Имеет ссылки на `PackedScenesContainer` (клиентская версия), `WorldEnvironment` и настройки игрока. При запуске инициализирует все необходимые сервисы, анализирует параметры командной строки и запускает игру.
        - `ClientRootSceneContainer.cs`: Отвечает за смену меню (`MainMenuMainScene`) на игру (`ClientGame`) и обратно.
        - `ClientRootSingleton.cs`: Реализация паттерна singleton для доступа к классу `ClientRoot` из любой точки проекта.
        - `PlayerSettings.cs`: Класс для работы с настройками игрока (загрузка из JSON файла и сохранение в JSON файл).
    - **ServerRoot**: Папка с файлами сцены `Root` для сервера.
        - `ServerRoot.tscn`: Техническая сцена без игровых объектов.
        - `ServerRoot.cs`: Имеет ссылки на `PackedScenesContainer` (серверная версия), `Console`. При запуске инициализирует все необходимые сервисы, анализирует параметры командной строки и запускает сервер.
        - `ServerRootSceneContainer.cs`: Отвечает за создание дочерней сцены (`ServerGame`).
        - `ServerRootSingleton.cs`: Реализация паттерна singleton для доступа к классу `ServerRoot` из любой точки проекта.
    - **StarterScene**: Папка с файлами сцены `StarterScene`. Общая для клиента и сервера.
        - `StarterScene.tscn`: Техническая сцена без игровых объектов. Сцена выбрана в качестве стартовой сцены в Godot.
        - `StarterScene.cs`: Имеет ссылки на прототипы сцен `ClientRoot` и `ServerRoot`. В зависимости от параметров командной строки заменяет загруженную `StarterScene.tscn` на `ClientRoot.tscn` или `ServerRoot.tscn`.
    - `RootService.cs`: Статичный класс с общими функциями, которые используются и на клиенте и на сервере. Отвечает за инициализацию сервисов использующихся и на клиенте и на сервере.

### Game
- **Game**: Общая папка со всеми файлами связанными со сценой `Game`
    - **ClientGame**: Папка с файлами сцены `Game` для клиента.
        - `ClientGame.cs`: Запускает функции инициализации из других `partial` классов `ClientGame`.
        - `ClientGameSceneContainer.cs`: Отвечает за переключение между `BattleWorldMainScene` и `SafeWorldMainScene`.
        - `ClientGameBaseNetwork.cs`: Отвечает за инициализацию сети и подключение к серверу.
        - `ClientGameNetworkListener.cs`: Отвечает за обработку сетевых пакетов с сообщениями верхнего уровня (начало игры, смена мира и т.п.).
        - `ClientGameLoadingScreen.cs`: Предоставляет возможность отображения загрузочного экрана.
        - `ClientGameServerShutdowner.cs`: Отвечает за добавление к сцене `ClientGame` сцены `ServerShutdowner`, которая завершает работу сервера при уничтожении сцены `ClientGame`.
        - **MainScenes**: Папка с файлами сцен типа `WorldMainScene` для клиента. Такая сцена объединяет в себе сцены `World` и `Hud`.
            - `IWorldMainScene.cs`: Общий интерфейс для всех сцен типа `WorldMainScene`
            - **BattleWorld**: Папка с файлами сцены типа `WorldMainScene` для боевого мира.
                - `BattleWorldMainScene.tscn`: Техническая сцена без игровых объектов. Содержит сцены `BattleWorld` и `BattleHud`.
                - `BattleWorldMainScene.cs`: Просто контейнер со ссылками на `BattleWorld` и `BattleHud`.
            - **SafeWorld**: Папка с файлами сцены типа `WorldMainScene` для безопасного мира.
                - `SafeWorldMainScene.tscn`: Техническая сцена без игровых объектов. Содержит сцены `SafeWorld` и `SafeHud`.
                - `SafeWorldMainScene.cs`: Просто контейнер со ссылками на `SafeWorld` и `SafeHud`.
    - **ServerGame**: Папка с файлами сцены `Game` для сервера.
        - `ServerGame.cs`: Запускает функции инициализации из других `partial` классов `ServerGame`.
        - `ServerGameSceneContainer.cs`: Отвечает за переключение между `ServerBattleWorld` и `ServerSafeWorld`.
        - `ServerGameBaseNetwork.cs`: Отвечает за инициализацию сети.
        - `ServerGameNetworkListener.cs`: Отвечает за обработку сетевых пакетов с сообщениями верхнего уровня (игрок подключен, игрок отключен, игрок хочет сменить мир и т.п.).

### PackedScenesContainer
- **PackedScenesContainer**:
    - **ClientPackedScenesContainer**:
        - `ClientPackedScenesContainer.tscn`: Техническая сцена без игровых объектов. Необходима, чтобы в редакторе Godot настраивать ссылки на прототипы других сцен клиента.
        - `ClientPackedScenesContainer.cs`: Содержит ссылки на прототипы других сцен клиента. Получение прототипа любой сцены (для последующего создания сцены), должно начинаться отсюда.
    - **ServerPackedScenesContainer**:
        - `ServerPackedScenesContainer.tscn`: Техническая сцена без игровых объектов. Необходима, чтобы в редакторе Godot настраивать ссылки на прототипы других сцен сервера.
        - `ServerPackedScenesContainer.cs`: Содержит ссылки на прототипы других сцен сервера. Получение прототипа любой сцены (для последующего создания сцены), должно начинаться отсюда.
