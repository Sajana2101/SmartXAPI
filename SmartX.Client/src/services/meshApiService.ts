
export interface MeshNode {
  id: string
  label: string
  zone: string
  isGateway: boolean
  isOnline: boolean
}

export interface MeshLink {
  from: string
  to: string
  cost: number
  rssiDbm: number
}

export interface MeshBfsResult {
  source: string
  visitOrder: string[]
  hopCounts: Record<string, number>
  parents: Record<string, string | null>
  unreachableOnline: string[]
  offline: string[]
}

export interface MeshTopology {
  gatewayId: string
  nodes: MeshNode[]
  links: MeshLink[]
  gatewayReachability: MeshBfsResult
  connectedComponents: string[][]
}

export interface MeshMstResult {
  gatewayId: string
  selectedLinks: MeshLink[]
  totalCost: number
  connectedNodes: string[]
  unreachableOnline: string[]
  offlineNodes: string[]
  onlineComponentCount: number
  spansAllOnlineNodes: boolean
}

export interface MeshRouteResult {
  source: string
  destination: string
  reachable: boolean
  path: string[]
  links: MeshLink[]
  totalCost: number | null
  hopCount: number
  message: string
}

export interface MeshRoutingOverview {
  minimumSpanningTree: MeshMstResult
  sensorRoutes: MeshRouteResult[]
}

export interface CreateMeshLinkRequest {
  from: string
  to: string
  cost: number
  rssiDbm: number
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7179'

async function request<T>(
  path: string,
  options?: RequestInit,
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, options)

  if (!response.ok) {
    let message = `Request failed (${response.status})`

    try {
      const data: unknown = await response.json()

      if (typeof data === 'object' && data !== null) {
        if ('message' in data && typeof data.message === 'string') {
          message = data.message
        } else if ('title' in data && typeof data.title === 'string') {
          message = data.title
        }
      }
    } catch {
      message = `Unable to complete request (${response.status})`
    }

    throw new Error(message)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export function getMeshTopology(): Promise<MeshTopology> {
  return request<MeshTopology>('/api/mesh')
}

export function getMeshRoutingOverview(): Promise<MeshRoutingOverview> {
  return request<MeshRoutingOverview>('/api/mesh/routing/overview')
}

export function createMeshLink(
  link: CreateMeshLinkRequest,
): Promise<MeshLink> {
  return request<MeshLink>('/api/mesh/links', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(link),
  })
}

export function deleteMeshLink(from: string, to: string): Promise<void> {
  const query = new URLSearchParams({ from, to })
  return request<void>(`/api/mesh/links?${query.toString()}`, {
    method: 'DELETE',
  })
}

export function updateMeshNodeStatus(
  id: string,
  isOnline: boolean,
): Promise<MeshTopology> {
  return request<MeshTopology>('/api/mesh/nodes/status', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ id, isOnline }),
  })
}
