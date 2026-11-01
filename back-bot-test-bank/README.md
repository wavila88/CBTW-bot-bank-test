# back-bot-test-bank 🎯

This is a base project implementing Hexagonal Architecture.  It can be copied and modified according to your specific needs, demonstrates how to implement **Hexagonal Architecture (Ports & Adapters)** in a C# application using .NET. The goal is to clearly separate business logic from infrastructure concerns, making the system more scalable, testable, and maintainable.
## Diagram

![hexagonalArquitecture](https://github.com/user-attachments/assets/eadfef19-bcbb-4b3f-9fad-674611170ed8)

## Youtube Video

[Link Youtube Video](https://youtu.be/-VZeZYS6MZA)

## 🧱 Project Structure

├── Api/ # Input adapter (controllers, DTOs) \
├── Application/ # Use cases and orchestration \
├── Domain/ # Entities, interfaces (ports), business logic \
├── Infra/ # Output adapters (repositories, messaging) \
