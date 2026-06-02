# DocuwareArchitect Sample

这是一个最小示例，模拟 DocuWare 的架构原理：

- `Platform.Identity`：身份/令牌模块，提供简单登录和令牌生成。
- `Platform.RestApi`：底层 REST API 层，暴露 `api/documents` 接口。
- `Platform.DotNetApi`：.NET API 封装层，作为 DLL 为上层提供 `DocuwareClient`。
- `Platform.WebClient`：MVC 平台层，调用 `Platform.DotNetApi` 获取数据并展示界面。
- `ThirdParty.Consumer`：第三方应用，直接引用 `Platform.DotNetApi` DLL，调用平台服务。

## 运行方式

1. 先启动 REST API：

   ```powershell
   dotnet run --project Platform.RestApi\Platform.RestApi.csproj
   ```

2. 启动 MVC 平台：

   ```powershell
   dotnet run --project Platform.WebClient\Platform.WebClient.csproj
   ```

3. 使用 Docker Compose 一键启动：

   ```powershell
   docker compose up --build
   ```

   - MVC 平台访问： http://localhost:5001
   - REST API 访问： http://localhost:5000/api/documents

4. 运行第三方调用示例：

   ```powershell
   dotnet run --project ThirdParty.Consumer\ThirdParty.Consumer.csproj
   ```

## 关键逻辑

- `Platform.RestApi` 提供了文档列表读取和新增接口。
- `Platform.DotNetApi` 将 REST API 调用封装为 `IDocuwareClient`，第三方和 MVC 都能共享该 DLL。
- `Platform.WebClient` 在 `HomeController` 中注入 `IDocuwareClient`，并通过 MVC 展示数据。
- `ThirdParty.Consumer` 直接实例化 `DocuwareClient` 并调用服务，模拟第三方使用 DLL 的场景。
