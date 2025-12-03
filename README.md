# 🧩 Microservices Ordering System — gRPC, RabbitMQ, Redis

This project demonstrates a robust **Microservices-Based Ordering System** built using .NET, gRPC, RabbitMQ, Redis, PostgreSQL, and React. It showcases how multiple independent services can communicate efficiently using gRPC and how asynchronous events are managed using message brokers.

The architecture ensures loose coupling, high scalability, and fast inter-service communication.

## 📘 Overview

### Primary Features
*   ✔ **Create Order** (Transactional consistency)
*   ✔ **Process Payment** (Mock payment gateway logic)
*   ✔ **Event-Driven Architecture** (Publish/Consume Payment Events via RabbitMQ)
*   ✔ **State Management** (Update Order Status asynchronously)
*   ✔ **Gateway Aggregation** (Return consolidated results to frontend)

## 🏗️ Architecture

### High-Level Diagram

```text
Frontend (React)
      |
      v
Gateway API (REST)
      |
      +--> gRPC --> Order Service
      |
      +--> gRPC --> Payment Service
                       |
                       v
                   RabbitMQ
                       |
                       v
                Order Service (Consumer)
                       |
                       v
              PostgreSQL Database


              📦 Technologies Used
Component	Purpose
gRPC	Fast service-to-service communication
RabbitMQ	Event-driven async messaging
Redis	Cache, token/session, fast key-value storage
PostgreSQL	Order storage database
Protobuf (.proto)	Contract-first schema for gRPC
ASP.NET	Gateway + Microservices
React (Vite)	Frontend
📂 Project Structure
code
Text
backend/
 ├── order-service/
 │     ├── Protos/order.proto
 │     ├── Services/OrderGrpcService.cs
 │     ├── Events/OrderPaymentCompletedConsumer.cs
 │     ├── Database/
 ├── payment-service/
 │     ├── Protos/payment.proto
 │     ├── Services/PaymentGrpcService.cs
 │     ├── Events/PaymentCompletedPublisher.cs
 ├── gateway-api/
 │     ├── Controllers/CheckoutController.cs
 │     ├── grpcClients/
frontend/
 └── vite-react-app/
🔄 Request Flow (Step-by-Step)
1️⃣ Frontend → Gateway API (REST)
User presses Buy Now, sending:
code
JSON
{
  "userId": 1,
  "amount": 100,
  "currency": "USD"
}
2️⃣ Gateway → Order Service (gRPC)
Gateway uses a gRPC client to call OrderService.CreateOrder().
Order Service: Saves order to PostgreSQL.
Returns: OrderId, Amount, Currency, Status.
3️⃣ Gateway → Payment Service (gRPC)
Gateway calls PaymentService.CreatePayment().
Payment Service: Generates Payment ID and Approval URL.
Returns: PaymentId, ApprovalUrl.
4️⃣ Payment Service → RabbitMQ (Event Publish)
After successful payment, the service publishes an event to the RabbitMQ exchange:
Event: payment.completed
5️⃣ Order Service → RabbitMQ (Event Consumer)
Order Service listens for the PaymentCompletedEvent.
Action: Updates Order Status: Pending → Confirmed.
Database: Order is updated in PostgreSQL.
6️⃣ Gateway → Frontend (REST Response)
code
JSON
{
  "orderId": 12,
  "paymentId": 55,
  "approvalUrl": "https://paypal.com/..."
}
🧠 Design Decisions
📘 Why gRPC?
gRPC is used for fast, internal microservice communication.
Feature	REST	gRPC
Format	JSON	Protobuf (binary)
Speed	Medium	Very fast
Contract Safety	No	Yes (.proto)
Streaming	Hard	Built-in
📘 Why RabbitMQ?
RabbitMQ handles asynchronous events to decouple services.
Updating order after payment.
Email/SMS services.
Inventory updates.
Logs and notifications.
📘 Why Redis?
Redis is used for performance and scalability:
✔ Caching frequently accessed data.
✔ User session/token storage.
✔ Fast key-value lookups.
✔ Reducing database load.
🔧 Running the Project
▶ Start Order Service
code
Bash
cd order-service
dotnet run
▶ Start Payment Service
code
Bash
cd payment-service
dotnet run
▶ Start Gateway API
code
Bash
cd gateway-api
dotnet run
▶ Start Frontend
code
Bash
cd frontend
npm install
npm run dev
📄 Protobuf Definitions
Protos help maintain strict contracts between microservices.
order.proto
code
Protobuf
service OrderService {
  rpc CreateOrder (CreateOrderRequest) returns (CreateOrderResponse);
}
payment.proto
code
Protobuf
service PaymentService {
  rpc CreatePayment (CreatePaymentRequest) returns (CreatePaymentResponse);
}
💡 Features Checklist

Microservice-based architecture

gRPC communication

RabbitMQ event-driven communication

PostgreSQL order persistence

Redis integration

Gateway REST API

CORS configured for frontend

Strongly typed Protobuf contracts