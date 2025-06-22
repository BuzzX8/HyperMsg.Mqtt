![HyperMsg Logo](logo.png)

# HyperMsg.Mqtt

## Project Design Summary

The HyperMsg.Mqtt solution is a modular MQTT protocol implementation in C#. It is organized into several projects:

- **HyperMsg.Mqtt**: Core library implementing MQTT packet structures, encoding/decoding logic, and protocol constants. Contains:
  - `Packets/`: MQTT packet definitions and properties.
  - `Coding/`: Encoding and decoding logic for MQTT packets.
  - `QosLevel.cs`: Quality of Service level definitions.

- **HyperMsg.Mqtt.Client**: Provides a high-level MQTT client API for connecting, publishing, and subscribing to MQTT brokers. Includes:
  - `MqttClient`: Main client class for managing connections and messaging.
  - `Internal/`: Connection management, packet ID handling, publishing, and subscription logic.
  - `ConnectionSettings`, `PublishRequest`, `WillMessageSettings`: Configuration and message options.

- **HyperMsg.MqttListener**: Implements a listener/server for accepting MQTT client connections. Handles incoming connections, manages session state, and processes MQTT protocol events.

- **test/**: Contains unit tests for both the core library and client components, ensuring protocol compliance and reliability.

This structure promotes separation of concerns, testability, and extensibility for MQTT-based messaging solutions.

---

