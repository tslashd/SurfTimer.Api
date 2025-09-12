# SurfTimer.Api

**SurfTimer.Api** is a .NET 8 RESTful API built for the SurfTimer solution.  
It provides a modern interface between the Counter-Strike 2 SurfTimer plugin and the MySQL database.

## ✨ Features
- **REST Endpoints**: Retrieve and manage SurfTimer data through HTTP requests.
- **Database Integration**: Communicates directly with a MySQL database using [MySqlConnector](https://mysqlconnector.net/) and [Dapper](https://www.learndapper.com).
- **Flexible Usage**:  
  - `SurfTimer.Plugin` can connect directly to MySQL.  
  - Or, for faster and more efficient communication, it can interact with this API.

## 🔗 Related Projects
- [`SurfTimer.Shared`](https://github.com/tslashd/SurfTimer.Shared) – core shared library required by the API.  
- [`SurfTimer.Plugin`](https://github.com/CS2Surf/Timer) – the Counter-Strike 2 plugin built on [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp).

