
export type AlertSeverity =
  | 'Critical'
  | 'High'
  | 'Medium'
  | 'Low'

export interface DeviceAlert {
  id: string
  sensorId: string
  severity: AlertSeverity
  message: string
  createdAtUtc: string
}

export interface DeviceAlertStatus {
  pendingCount: number
  processed: DeviceAlert[]
}

export interface EnqueueAlertRequest {
  sensorId: string
  severity: AlertSeverity
  message: string
}
