System: «Напиши код рабочего mid‑frequency торгового бота под MOEX.
Constraints:
– Язык C# 8.0, .NET 8, cross‑platform.
– Используй пакет QuikSharp для соединения с QUIK, WebSocketSharp для резервного фида.
– Архитектура DDD: Domain, Infrastructure, Application, UI.
– Входные данные: L1 Quote, Level2Quote, Trades, внешний REST‑фид Brent & USD/RUB, JSON‑новости.
– Реализуй 4 сервиса:
1. FeaturePipeline (скользящие дельты, z‑score базиса, SMA‑волатильность);
2. SignalService (правило: if z_score > 1.5 && sentiment==Bull → Buy; <-1.5 && sentiment==Bear → Sell);
3. OrderExecutor (IOC‑лимитная заявка, пересчёт ГО, слежение за исполнением, отмена через 3 s);
4. RiskGuardian (VAR 99%, стоп –0.7 % equity, контроль слippage ≤ 0.4 % edge).
– Сделай конфиг в appsettings.json: брокер‑логин, список тикеров, maxPosition, VPSLatencyMs.
– Добавь консольный UI с командами start, stop, status.
– Покрой юнит‑тестами SignalService (NUnit).
– Код должен компилироваться dotnet build без внешних зависимостей кроме NuGet‑пакетов.
– Выводи лог в Prometheus‑формате.
User: «Сгенерируй решение».
