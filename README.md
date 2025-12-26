# HyperMsg.Mqtt

**A lightweight, test-driven MQTT v5 client library for .NET.**

HyperMsg.Mqtt provides a clear, modular implementation of the MQTT 5 protocol, including packet encoding/decoding, a client context, and convenience components for connection, publishing and subscription management. It's designed for correctness, ease of testing, and adaptability in both client and embedded scenarios.

## Key Features ✅

- Full MQTT 5 packet model and encoding/decoding
- Client abstraction with `MqttClient` and `ClientContext`
- Modular components for connection, publishing and subscriptions
- Comprehensive unit tests and test fixtures

---

## Quick Start 🔧

1. Build the solution:

   ```bash
   dotnet build
   ```

2. Run tests:

   ```bash
   dotnet test
   ```

3. Explore projects under `src/` (e.g. `HyperMsg.Mqtt.Client`, `HyperMsg.Mqtt.Coding`, `HyperMsg.Mqtt.Packets`).

---

## Contributing 💡

Contributions, bug reports, and enhancements are welcome. Please open issues or pull requests and follow the existing test-driven approach.

## License

This project is open source — include your license here (e.g. MIT).
