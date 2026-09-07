export type SensorCategory =
  | 'Environmental'
  | 'PowerConsumption'
  | 'Actuator'

export interface Sensor {
  id: string
  deviceIdentifier: string
  deploymentLocation: string
  room: string
  zone: string
  nodeId: string
  category: SensorCategory
  createdAtUtc: string
  updatedAtUtc: string
}

export interface SensorRequest {
  deviceIdentifier: string
  deploymentLocation: string
  room: string
  zone: string
  nodeId: string
  category: SensorCategory
}