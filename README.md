# 🚀 Microservices Architecture – gRPC + RabbitMQ + Redis + PostgreSQL

This project demonstrates a complete microservice-based architecture using:

* **gRPC** for fast internal communication
* **RabbitMQ** for event-driven messaging
* **PostgreSQL** for order persistence
* **Redis** for caching and token/session storage
* **REST Gateway API** for frontend communication
* **React/Next.js frontend**

---

# 📌 Architecture Overview

```
Frontend (REST)
      ↓
Gateway API (REST → gRPC)
      ↓
 ┌───────────────┬─────────────────┐
 │ Order Service  │ Payment Service │
 │   (gRPC)       │     (gRPC)      │
 │  PostgreSQL    │   PayPal SDK    │
 └───────┬────────┴─────────────┬──┘
         │                      │
         └────── RabbitMQ (Events) ────────→ Order Updated
```

---

# 🔄 Request Flow (Step-by-Step)

## 1️⃣ Frontend → Gateway API (REST)

User clicks **Buy Now**, frontend sends:

```json
{
  "userId": 1,
  "amount": 100,
  "currency": "USD"
}
```

## 2️⃣ Gateway → Order Service (gRPC)

Gateway calls:

```
OrderService.CreateOrder()
```

Order Service:

* Saves order to PostgreSQL
* Returns:

```json
{
  "orderId": 12,
  "amount": 100,
  "currency": "USD",
  "status": "PENDING"
}
```

## 3️⃣ Gateway → Payment Service (gRPC)

Gateway calls:

```
PaymentService.CreatePayment()
```

Payment Service:

* Generates PaymentID
* Returns PayPal approval URL

```json
{
  "paymentId": 55,
  "approvalUrl": "https://paypal.com/..."
}
```

## 4️⃣ Payment Service → RabbitMQ

Event published:

```
payment.completed
```

Payload:

```json
{
  "orderId": 12,
  "paymentId": 55,
  "status": "COMPLETED"
}
```

## 5️⃣ Order Service → RabbitMQ (Consumer)

Order Service listens and updates order status in DB.

## 6️⃣ Gateway → Frontend (REST Response)

```json
{
  "orderId": 12,
  "paymentId": 55,
  "approvalUrl": "https://paypal.com/..."
}
```

---

# 🧠 Design Decisions

## 📘 Why gRPC?

| Feature         | REST   | gRPC      |
| --------------- | ------ | --------- |
| Format          | JSON   | Protobuf  |
| Speed           | Medium | Very Fast |
| Contract Safety | No     | Yes       |
| Streaming       | Hard   | Built-in  |

## 📘 Why RabbitMQ?

* Enables async event-driven communication
* Order/payment updates
* Notifications, logs, inventory

## 📘 Why Redis?

* High-speed caching
* Token/session storage
* Reduces DB load
* TTL-based temporary data

---

# 🔧 Running the Project

## ▶ Order Service

```bash
cd order-service
dotnet run
```

## ▶ Payment Service

```bash
cd payment-service
dotnet run
```

## ▶ Gateway API

```bash
cd gateway-api
dotnet run
```

## ▶ Frontend

```bash
cd frontend
npm install
npm run dev
```

---

# 📄 Protobuf Definitions

## **order.proto**

```proto
syntax = "proto3";

option csharp_namespace = "OrderProto";

package order;

service OrderService {
  rpc CreateOrder (CreateOrderRequest) returns (CreateOrderResponse);
}

message CreateOrderRequest {
  int32 userId = 1;
  double amount = 2;
  string currency = 3;
}

message CreateOrderResponse {
  int32 orderId = 1;
  double amount = 2;
  string currency = 3;
  string status = 4;
}
```

## **payment.proto**

```proto
syntax = "proto3";

option csharp_namespace = "PaymentProto";

package payment;

service PaymentService {
  rpc CreatePayment (CreatePaymentRequest) returns (CreatePaymentResponse);
}

message CreatePaymentRequest {
  int32 orderId = 1;
  double amount = 2;
  string currency = 3;
}

message CreatePaymentResponse {
  int32 paymentId = 1;
  string approvalUrl = 2;
}
```

---

# ✅ Features Checklist

* Microservice Architecture
* gRPC Communication
* RabbitMQ Messaging
* PostgreSQL Persistence
* Redis Integration
* REST Gateway API
* Frontend Integration
* CORS Configured
* Strong Protobuf Contracts
