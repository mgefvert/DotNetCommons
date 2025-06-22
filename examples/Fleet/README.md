# Fleet Command System

A demonstration of the DotNetCommons command architecture, Fleet is a simulated military command and control system for
managing naval vessels and air wings.

## Overview

The Fleet Command System allows operators to manage the deployment status and readiness conditions of military units through a 
command-line interface. It showcases how to implement a command pattern using the DotNetCommons framework.

## Features

- **Unit Management**: Track submarines and air wings
- **Deployment Operations**: Deploy and recall units individually or in groups
- **Readiness Control**: Set and monitor DEFCON and REDCON levels
- **Emergency Protocols**: Implement high-priority commands like "Broken Arrow"
- **Persistent State**: Unit status is saved between sessions

## Commands

### Basic Commands

Note: -u accepts wildcards such as `?` or `*`.

- `deploy`: Deploy units to active status
  - `-u, --unit <name>`: Specify unit(s) to deploy
  - `-a, --all`: Deploy all units

- `recall`: Return units to non-deployed status
  - `-u, --unit <name>`: Specify unit(s) to recall
  - `-a, --all`: Recall all units

- `set defcon`: Set Defense Condition level
  - `-u, --unit <name>`: Apply to specific unit(s)
  - `-a, --all`: Apply to all units
  - `-v, --value <level>`: Level (1-5)

- `set redcon`: Set Readiness Condition level
  - `-u, --unit <name>`: Apply to specific unit(s)
  - `-a, --all`: Apply to all units
  - `-v, --value <level>`: Level (1-5)

- `list units`: Display all units and their current status

- `nuke`: Nuke a specific target
  - `-u, --unit <name>`: Apply to specific unit(s)
  - `-a, --all`: Apply to all units
  - `-t, --target <name>`: Specify target


### Emergency Protocols

- `broken arrow`: Emergency command that deploys all units and sets maximum readiness levels

## Architecture

The system demonstrates:

- Command pattern implementation
- Dependency injection
- Command chaining
- Structured logging
- Persistent state management

## Usage Example

```bash
# Deploy a specific submarine
./fleet deploy -u ohio-1

# Recall all units
./fleet recall -a

# Set DEFCON 2 for all units
./fleet set defcon 2 -a

# Emergency protocol
./fleet broken arrow
```
