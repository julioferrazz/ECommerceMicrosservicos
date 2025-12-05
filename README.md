# 🛒 Sistema E-Commerce - Microsserviços

> Sistema de e-commerce distribuído baseado em arquitetura de microsserviços utilizando .NET 8, SQL Server, RabbitMQ e API Gateway com autenticação JWT.



## 🎯 Visão Geral

Sistema de e-commerce desenvolvido com arquitetura de microsserviços, permitindo escalabilidade, manutenibilidade e separação de responsabilidades. O sistema gerencia produtos, estoque e vendas de forma distribuída, utilizando comunicação síncrona (HTTP) e assíncrona (mensageria com RabbitMQ).

---

## ✨ Funcionalidades

### 🔐 Autenticação e Autorização
- **Login com JWT**: Autenticação de usuários com geração de token JWT
- **Proteção de Rotas**: Apenas usuários autenticados podem acessar os microsserviços
- **Validação de Token**: Validação automática em todas as requisições protegidas

### 📦 Gestão de Estoque (EstoqueService)
- **Cadastro de Produtos**: Adicionar novos produtos com nome, descrição, preço e quantidade
- **Consulta de Produtos**: Listar todos os produtos disponíveis
- **Consulta Individual**: Visualizar detalhes de um produto específico
- **Verificação de Disponibilidade**: Validar se há estoque suficiente para venda
- **Atualização Automática**: Estoque atualizado via mensageria após confirmação de pedidos
- **Auditoria**: Registro de data de cadastro e última atualização

### 🛍️ Gestão de Vendas (VendasService)
- **Criação de Pedidos**: Registrar pedidos com múltiplos itens
- **Validação de Estoque**: Verificação automática de disponibilidade antes de confirmar
- **Cálculo de Valores**: Cálculo automático de subtotais e valor total do pedido
- **Consulta de Pedidos**: Visualizar pedidos individuais ou listar todos
- **Status de Pedidos**: Acompanhamento do status (Pendente, Confirmado, Cancelado, Entregue)
- **Notificação Assíncrona**: Comunicação com serviço de estoque via RabbitMQ

### 🌐 API Gateway
- **Ponto Único de Entrada**: Centralização de todas as requisições
- **Roteamento Inteligente**: Direcionamento automático para o microsserviço correto
- **Autenticação Centralizada**: Validação de JWT no gateway
- **Balanceamento**: Distribuição de carga entre instâncias (configurável)

---

## 📜 Regras de Negócio

### Produtos
1. **Cadastro de Produto**
   - Nome é obrigatório (máx. 200 caracteres)
   - Descrição é opcional (máx. 1000 caracteres)
   - Preço deve ser maior que zero
   - Quantidade inicial de estoque deve ser informada
   - Data de cadastro é registrada automaticamente

2. **Controle de Estoque**
   - Quantidade em estoque não pode ser negativa
   - Estoque é decrementado automaticamente após confirmação de pedido
   - Atualização de estoque registra timestamp da modificação

### Pedidos
1. **Criação de Pedido**
   - Cliente deve informar nome e email
   - Pedido deve conter ao menos 1 item
   - Sistema valida disponibilidade de todos os produtos antes de confirmar
   - Se algum produto não tiver estoque suficiente, pedido é rejeitado
   - Preço do produto é capturado no momento do pedido (histórico de preço)

2. **Validação de Estoque**
   - Verificação em tempo real antes de confirmar pedido
   - Consulta ao EstoqueService via HTTP
   - Transação não confirmada se houver inconsistência

3. **Processamento de Pedido**
   - Status inicial: **Pendente**
   - Após validação: **Confirmado**
   - Mensagem enviada ao RabbitMQ para atualização de estoque
   - EstoqueService consome mensagem e atualiza quantidades

4. **Cálculos Financeiros**
   - Subtotal = Preço Unitário × Quantidade
   - Valor Total = Soma de todos os subtotais
   - Valores armazenados com precisão decimal (18,2)

### Autenticação
1. **Login**
   - Credenciais: username e password
   - Token JWT gerado com validade de 2 horas
   - Token deve ser incluído no header Authorization de todas as requisições protegidas

2. **Segurança**
   - Endpoints de produtos e pedidos requerem autenticação
   - Token validado em cada requisição
   - Chave secreta compartilhada entre microsserviços

---

## 🏗️ Arquitetura

### Arquitetura de Microsserviços

```
┌─────────────────────────────────────────────────────┐
│                     Cliente                         │
│              (Postman / Frontend)                   │
└────────────────────┬────────────────────────────────┘
                     │ HTTP/HTTPS
                     ▼
┌─────────────────────────────────────────────────────┐
│                  API Gateway                        │
│              (Port 5000)                            │
│  • Autenticação JWT                                 │
│  • Roteamento                                       │
│  • Validação de Token                               │
└──────────────┬──────────────────┬───────────────────┘
               │                  │
       ┌───────▼─────┐    ┌──────▼────────┐
       │             │    │               │
       │  Estoque    │    │    Vendas     │
       │  Service    │◄───┤    Service    │
       │ (Port 5001) │    │  (Port 5002)  │
       │             │    │               │
       └──────┬──────┘    └───────┬───────┘
              │                   │
              │  ┌────────────────┘
              │  │
              ▼  ▼
       ┌──────────────┐        ┌──────────────┐
       │              │        │              │
       │  SQL Server  │        │  RabbitMQ    │
       │ db_sistema   │        │  (Message    │
       │    venda     │        │   Broker)    │
       │              │        │              │
       └──────────────┘        └──────────────┘
```

### Comunicação entre Serviços

**Comunicação Síncrona (HTTP)**
- VendasService → EstoqueService: Validação de disponibilidade
- Cliente → API Gateway → Microsserviços

**Comunicação Assíncrona (RabbitMQ)**
- VendasService → EstoqueService: Notificação de venda confirmada
- Padrão: Publisher/Subscriber
- Fila: `estoque_queue` (durable)

### Padrões Arquiteturais Utilizados

- **API Gateway Pattern**: Ponto único de entrada
- **Database per Service**: Cada microsserviço com seu contexto de dados
- **Event-Driven Architecture**: Comunicação assíncrona via eventos
- **Repository Pattern**: Abstração de acesso a dados com Entity Framework
- **DTO Pattern**: Transferência de dados entre camadas
- **Dependency Injection**: Inversão de controle nativa do .NET

---

## 🛠️ Tecnologias e Pacotes

### Frameworks e Linguagens
- **.NET 8.0**: Framework principal
- **C# 12.0**: Linguagem de programação
- **ASP.NET Core Web API**: Desenvolvimento de APIs RESTful

### Banco de Dados
- **SQL Server 2022**: Banco de dados relacional
- **Entity Framework Core 8.0**: ORM para acesso a dados
  - `Microsoft.EntityFrameworkCore` (8.0.0)
  - `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)
  - `Microsoft.EntityFrameworkCore.Tools` (8.0.0)

### Mensageria
- **RabbitMQ 3.12**: Message broker
  - `RabbitMQ.Client` (6.8.1)

### Autenticação e Segurança
- **JWT (JSON Web Tokens)**: Autenticação stateless
  - `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.0)
  - `System.IdentityModel.Tokens.Jwt`

### API Gateway
- **Ocelot 23.2.2**: API Gateway e roteamento
  - Roteamento de requisições
  - Agregação de serviços
  - Autenticação e autorização

---

## 📁 Estrutura do Projeto

```
ECommerceMicroservices/
├── src/
│   ├── Shared/
│   │   └── Shared/
│   │       ├── Messages/
│   │       │   └── AtualizacaoEstoqueMessage.cs
│   │       └── Shared.csproj
│   │
│   ├── EstoqueService/
│   │   └── EstoqueService/
│   │       ├── Controllers/
│   │       │   └── ProdutosController.cs
│   │       ├── Data/
│   │       │   └── EstoqueContext.cs
│   │       ├── Models/
│   │       │   └── Produto.cs
│   │       ├── DTOs/
│   │       │   └── ProdutoDto.cs
│   │       ├── Services/
│   │       │   └── RabbitMQConsumerService.cs
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── EstoqueService.csproj
│   │
│   ├── VendasService/
│   │   └── VendasService/
│   │       ├── Controllers/
│   │       │   └── PedidosController.cs
│   │       ├── Data/
│   │       │   └── VendasContext.cs
│   │       ├── Models/
│   │       │   ├── Pedido.cs
│   │       │   └── ItemPedido.cs
│   │       ├── DTOs/
│   │       │   └── PedidoDto.cs
│   │       ├── Services/
│   │       │   ├── RabbitMQPublisherService.cs
│   │       │   └── EstoqueHttpService.cs
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── VendasService.csproj
│   │
│   └── ApiGateway/
│       └── ApiGateway/
│           ├── Controllers/
│           │   └── AuthController.cs
│           ├── Models/
│           │   └── Usuario.cs
│           ├── Program.cs
│           ├── ocelot.json
│           ├── appsettings.json
│           └── ApiGateway.csproj
│
└── README.md
```
