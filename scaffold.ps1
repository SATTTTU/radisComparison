# Create Solution
dotnet new sln -n BackendSolution -o . --force

# Create Projects
dotnet new webapi -n gateway-api -o gateway-api --force
dotnet new grpc -n order-service -o order-service --force
dotnet new grpc -n payment-service -o payment-service --force
dotnet new classlib -n shared -o shared --force

# Add Projects to Solution
dotnet sln add gateway-api/gateway-api.csproj
dotnet sln add order-service/order-service.csproj
dotnet sln add payment-service/payment-service.csproj
dotnet sln add shared/shared.csproj

# Add Project References
dotnet add gateway-api/gateway-api.csproj reference shared/shared.csproj
dotnet add order-service/order-service.csproj reference shared/shared.csproj
dotnet add payment-service/payment-service.csproj reference shared/shared.csproj

# Add NuGet Packages
# Gateway API
dotnet add gateway-api/gateway-api.csproj package Grpc.Net.Client
dotnet add gateway-api/gateway-api.csproj package Grpc.Tools
dotnet add gateway-api/gateway-api.csproj package Google.Protobuf
dotnet add gateway-api/gateway-api.csproj package Swashbuckle.AspNetCore

# Order Service
dotnet add order-service/order-service.csproj package Grpc.Tools
dotnet add order-service/order-service.csproj package Google.Protobuf
dotnet add order-service/order-service.csproj package Microsoft.EntityFrameworkCore
dotnet add order-service/order-service.csproj package Microsoft.EntityFrameworkCore.SqlServer

# Payment Service
dotnet add payment-service/payment-service.csproj package Grpc.Tools
dotnet add payment-service/payment-service.csproj package Google.Protobuf
dotnet add payment-service/payment-service.csproj package PayPalCheckoutSdk

# Create Folder Structures
New-Item -ItemType Directory -Force -Path "gateway-api/Controllers"
New-Item -ItemType Directory -Force -Path "gateway-api/GrpcClients"
New-Item -ItemType Directory -Force -Path "gateway-api/Protos"

New-Item -ItemType Directory -Force -Path "order-service/Domain"
New-Item -ItemType Directory -Force -Path "order-service/Data"
New-Item -ItemType Directory -Force -Path "order-service/Repositories"
New-Item -ItemType Directory -Force -Path "order-service/Services"
New-Item -ItemType Directory -Force -Path "order-service/Mappers"
New-Item -ItemType Directory -Force -Path "order-service/Protos"

New-Item -ItemType Directory -Force -Path "payment-service/Services"
New-Item -ItemType Directory -Force -Path "payment-service/Config"
New-Item -ItemType Directory -Force -Path "payment-service/Protos"

New-Item -ItemType Directory -Force -Path "shared/Models"
New-Item -ItemType Directory -Force -Path "shared/Helpers"
New-Item -ItemType Directory -Force -Path "shared/Extensions"
