
import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import {
  createMeshLink,
  deleteMeshLink,
  getMeshRoutingOverview,
  getMeshTopology,
  updateMeshNodeStatus,
} from '../services/meshApiService'
import type {
  MeshLink,
  MeshRoutingOverview,
  MeshTopology,
} from '../services/meshApiService'
import './MeshTopologyPage.css'

interface MeshTopologyPageProps {
  onBack: () => void
}

type DisplayMode = 'route' | 'mst'

interface Position {
  x: number
  y: number
}

function linkKey(from: string, to: string): string {
  return [from.toUpperCase(), to.toUpperCase()].sort().join('|')
}

function MeshTopologyPage({ onBack }: MeshTopologyPageProps) {
  const [topology, setTopology] = useState<MeshTopology | null>(null)
  const [routing, setRouting] = useState<MeshRoutingOverview | null>(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [selectedId, setSelectedId] = useState('')
  const [displayMode, setDisplayMode] = useState<DisplayMode>('route')
  const [from, setFrom] = useState('GATEWAY-01')
  const [to, setTo] = useState('')
  const [cost, setCost] = useState(2.5)
  const [rssi, setRssi] = useState(-60)

  const refresh = useCallback(async () => {
    try {
      const [network, routes] = await Promise.all([
        getMeshTopology(),
        getMeshRoutingOverview(),
      ])
      setTopology(network)
      setRouting(routes)
      setError('')
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to load the mesh network.',
      )
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    const initialTimer = window.setTimeout(() => {
      void refresh()
    }, 0)

    const timer = window.setInterval(() => {
      void refresh()
    }, 5000)

    return () => {
      window.clearTimeout(initialTimer)
      window.clearInterval(timer)
    }
  }, [refresh])

  const sensors = useMemo(
    () => topology?.nodes.filter(node => !node.isGateway) ?? [],
    [topology],
  )

  const activeSelectedId = sensors.some(
    node => node.id.toLowerCase() === selectedId.toLowerCase(),
  )
    ? selectedId
    : sensors[0]?.id ?? ''

  const selectedSensor = sensors.find(
    node => node.id.toLowerCase() === activeSelectedId.toLowerCase(),
  )

  const selectedRoute = routing?.sensorRoutes.find(
    route => route.source.toLowerCase() === activeSelectedId.toLowerCase(),
  )

  const mst = routing?.minimumSpanningTree

  const highlightedLinks = useMemo(() => {
    const links = displayMode === 'mst'
      ? routing?.minimumSpanningTree.selectedLinks ?? []
      : routing?.sensorRoutes.find(
          route => route.source.toLowerCase() === activeSelectedId.toLowerCase(),
        )?.links ?? []

    return new Set(
      links.map(link => linkKey(link.from, link.to)),
    )
  }, [routing, displayMode, activeSelectedId])

  
  const positions = useMemo(() => {
    const result = new Map<string, Position>()
    if (!topology) {
      return result
    }

    const gateway = topology.nodes.find(node => node.isGateway)
    const devices = topology.nodes.filter(node => !node.isGateway)

    if (devices.length === 2) {
      if (gateway) {
        result.set(gateway.id.toLowerCase(), { x: 460, y: 385 })
      }

      result.set(devices[0].id.toLowerCase(), { x: 230, y: 150 })
      result.set(devices[1].id.toLowerCase(), { x: 690, y: 150 })

      return result
    }

    if (gateway) {
      result.set(gateway.id.toLowerCase(), { x: 460, y: 260 })
    }

    devices.forEach((node, index) => {
      const ring = Math.floor(index / 12)
      const start = ring * 12
      const count = Math.min(12, devices.length - start)
      const position = index - start
      const angle = -Math.PI / 2 + (2 * Math.PI * position) / count
      const radiusX = Math.max(140, 310 - ring * 55)
      const radiusY = Math.max(100, 185 - ring * 35)

      result.set(node.id.toLowerCase(), {
        x: 460 + Math.cos(angle) * radiusX,
        y: 260 + Math.sin(angle) * radiusY,
      })
    })

    return result
  }, [topology])


  const nodeLookup = useMemo(
    () => new Map(
      topology?.nodes.map(node => [node.id.toLowerCase(), node]) ?? [],
    ),
    [topology],
  )

  async function handleAddLink(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (busy || !from || !to) {
      return
    }

    if (from.toLowerCase() === to.toLowerCase()) {
      setError('Choose two different nodes.')
      return
    }

    if (!Number.isFinite(cost) || cost <= 0) {
      setError('Connection cost must be greater than zero.')
      return
    }

    setBusy(true)
    setNotice('')
    setError('')

    try {
      await createMeshLink({
        from,
        to,
        cost,
        rssiDbm: rssi,
      })
      await refresh()
      setNotice('Wireless connection created.')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not create link.')
    } finally {
      setBusy(false)
    }
  }

  async function handleRemoveLink(link: MeshLink) {
    if (busy) {
      return
    }

    setBusy(true)
    setError('')
    setNotice('')

    try {
      await deleteMeshLink(link.from, link.to)
      await refresh()
      setNotice('Wireless connection removed.')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not remove link.')
    } finally {
      setBusy(false)
    }
  }

  async function handleToggleNode(id: string, isOnline: boolean) {
    if (busy) {
      return
    }

    setBusy(true)
    setError('')
    setNotice('')

    try {
      await updateMeshNodeStatus(id, !isOnline)
      await refresh()
      setNotice(
        !isOnline
          ? 'Sensor restored. Routes have been recalculated.'
          : 'Sensor disabled. Routes have been recalculated.',
      )
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Could not update sensor.',
      )
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="mesh-page">
      <div className="mesh-shell">
        <header className="mesh-header">
          <div>
            <button className="mesh-back" type="button" onClick={onBack}>
              Back to Smart-X Home
            </button>
            <p className="mesh-eyebrow">SMART-X / PART 3</p>
            <h1>Mesh Network Topology</h1>
            <p>
              Monitor sensor connections, identify disconnected nodes,
              and simulate resilient gateway routing.
            </p>
          </div>
          <button
            className="mesh-button mesh-button-secondary"
            type="button"
            onClick={() => void refresh()}
          >
            Refresh Network
          </button>
        </header>

        {error && (
          <div className="mesh-alert mesh-alert-error" role="alert">
            {error}
          </div>
        )}

        {notice && (
          <div className="mesh-alert mesh-alert-success" role="status">
            {notice}
          </div>
        )}

        {loading && !topology && (
          <div className="mesh-card">Loading network topology...</div>
        )}

        {topology && routing && (
          <main className="mesh-main">
            <section className="mesh-stats">
              <article className="mesh-stat">
                <span>Registered sensors</span>
                <strong>{sensors.length}</strong>
              </article>
              <article className="mesh-stat">
                <span>Wireless links</span>
                <strong>{topology.links.length}</strong>
              </article>
              <article className="mesh-stat">
                <span>Reachable sensors</span>
                <strong>
                  {topology.gatewayReachability.visitOrder.length - 1}
                </strong>
              </article>
              <article className="mesh-stat">
                <span>Offline sensors</span>
                <strong>{topology.gatewayReachability.offline.length}</strong>
              </article>
              <article className="mesh-stat">
                <span>MST total cost</span>
                <strong>{mst?.totalCost.toFixed(2) ?? '0.00'}</strong>
              </article>
            </section>

            <section className="mesh-layout">
              <div className="mesh-card mesh-map-card">
                <div className="mesh-section-heading">
                  <div>
                    <h2>Network Map</h2>
                    <p>Click a sensor to inspect its gateway route.</p>
                  </div>
                  <div className="mesh-mode-toggle">
                    <button
                      type="button"
                      className={displayMode === 'route' ? 'active' : ''}
                      onClick={() => setDisplayMode('route')}
                    >
                      Shortest Route
                    </button>
                    <button
                      type="button"
                      className={displayMode === 'mst' ? 'active' : ''}
                      onClick={() => setDisplayMode('mst')}
                    >
                      Prim's MST
                    </button>
                  </div>
                </div>

                <div className="mesh-canvas">
                  <svg
                    viewBox="0 0 920 540"
                    role="img"
                    aria-label="Wireless mesh network with sensors and gateway"
                  >
                    {topology.links.map(link => {
                      const start = positions.get(link.from.toLowerCase())
                      const end = positions.get(link.to.toLowerCase())

                      if (!start || !end) {
                        return null
                      }

                      const startNode = nodeLookup.get(
                        link.from.toLowerCase(),
                      )
                      const endNode = nodeLookup.get(link.to.toLowerCase())
                      const offline = !startNode?.isOnline || !endNode?.isOnline
                      const highlighted = highlightedLinks.has(
                        linkKey(link.from, link.to),
                      )

                      return (
                        <g key={linkKey(link.from, link.to)}>
                          <line
                            x1={start.x}
                            y1={start.y}
                            x2={end.x}
                            y2={end.y}
                            stroke={
                              offline
                                ? '#cbd5e1'
                                : highlighted
                                  ? '#2563eb'
                                  : '#94a3b8'
                            }
                            strokeWidth={highlighted ? 6 : 3}
                            strokeDasharray={offline ? '9 7' : undefined}
                            opacity={offline ? 0.45 : 0.9}
                          />
                          <rect
                            x={(start.x + end.x) / 2 - 20}
                            y={(start.y + end.y) / 2 - 12}
                            width={40}
                            height={24}
                            rx={9}
                            fill="#ffffff"
                            stroke="#e2e8f0"
                          />
                          <text
                            x={(start.x + end.x) / 2}
                            y={(start.y + end.y) / 2 + 5}
                            textAnchor="middle"
                            className="mesh-edge-label"
                          >
                            {link.cost.toFixed(1)}
                          </text>
                        </g>
                      )
                    })}

                    {topology.nodes.map(node => {
                      const point = positions.get(node.id.toLowerCase())

                      if (!point) {
                        return null
                      }

                      const selected =
                        node.id.toLowerCase() === activeSelectedId.toLowerCase()

                      return (
                        <g
                          key={node.id}
                          className="mesh-svg-node"
                          role={node.isGateway ? undefined : 'button'}
                          tabIndex={node.isGateway ? undefined : 0}
                          onClick={() => {
                            if (!node.isGateway) {
                              setSelectedId(node.id)
                              setDisplayMode('route')
                            }
                          }}
                          onKeyDown={event => {
                            if (
                              !node.isGateway &&
                              (event.key === 'Enter' || event.key === ' ')
                            ) {
                              event.preventDefault()
                              setSelectedId(node.id)
                              setDisplayMode('route')
                            }
                          }}
                        >
                          <title>{`${node.label} (${node.id})`}</title>
                          <circle
                            cx={point.x}
                            cy={point.y}
                            r={node.isGateway ? 35 : selected ? 31 : 27}
                            fill={
                              node.isGateway
                                ? '#1e3a8a'
                                : !node.isOnline
                                  ? '#fee2e2'
                                  : selected
                                    ? '#dbeafe'
                                    : '#dcfce7'
                            }
                            stroke={
                              node.isGateway
                                ? '#1d4ed8'
                                : !node.isOnline
                                  ? '#ef4444'
                                  : selected
                                    ? '#2563eb'
                                    : '#16a34a'
                            }
                            strokeWidth={selected ? 5 : 3}
                          />
                          <text
                            x={point.x}
                            y={point.y + 5}
                            textAnchor="middle"
                            className="mesh-node-symbol"
                            fill={node.isGateway ? '#fff' : '#1e293b'}
                          >
                            {node.isGateway ? 'GW' : node.isOnline ? 'S' : 'X'}
                          </text>
                          <text
                            x={point.x}
                            y={point.y + (node.isGateway ? 55 : 48)}
                            textAnchor="middle"
                            className="mesh-node-label"
                          >
                            {node.label.length > 18
                              ? `${node.label.slice(0, 17)}…`
                              : node.label}
                          </text>
                        </g>
                      )
                    })}
                  </svg>
                </div>

                <div className="mesh-legend">
                  <span><i className="mesh-key mesh-key-gateway" />Gateway</span>
                  <span><i className="mesh-key mesh-key-online" />Online</span>
                  <span><i className="mesh-key mesh-key-offline" />Offline</span>
                  <span><i className="mesh-key mesh-key-route" />Highlighted</span>
                </div>

                {sensors.length === 0 && (
                  <p className="mesh-empty">
                    No sensors registered. Use Sensor Management to create
                    sensors, then return here to connect them to the gateway.
                  </p>
                )}
              </div>

              <aside className="mesh-card mesh-detail">
                <h2>Routing Inspector</h2>
                <label htmlFor="mesh-selected-sensor">Selected sensor</label>
                <select
                  id="mesh-selected-sensor"
                  value={activeSelectedId}
                  onChange={event => setSelectedId(event.target.value)}
                >
                  {sensors.length === 0 && (
                    <option value="">No sensors available</option>
                  )}
                  {sensors.map(node => (
                    <option key={node.id} value={node.id}>
                      {node.label} — {node.id}
                    </option>
                  ))}
                </select>

                {selectedSensor && (
                  <div className="mesh-route-details">
                    <div className="mesh-detail-row">
                      <span>Status</span>
                      <strong>
                        {selectedSensor.isOnline ? 'Online' : 'Offline'}
                      </strong>
                    </div>
                    <div className="mesh-detail-row">
                      <span>Zone</span>
                      <strong>{selectedSensor.zone}</strong>
                    </div>
                    <div className="mesh-detail-row">
                      <span>Reachability</span>
                      <strong>
                        {selectedRoute?.reachable
                          ? 'Connected'
                          : 'Disconnected'}
                      </strong>
                    </div>
                    <div className="mesh-detail-row">
                      <span>Route cost</span>
                      <strong>
                        {selectedRoute?.totalCost == null
                          ? 'Unavailable'
                          : selectedRoute.totalCost.toFixed(2)}
                      </strong>
                    </div>
                    <div className="mesh-detail-row">
                      <span>Hops</span>
                      <strong>{selectedRoute?.hopCount ?? 0}</strong>
                    </div>

                    <div className="mesh-path">
                      <span>Dijkstra's shortest-cost route</span>
                      <p>
                        {selectedRoute?.reachable
                          ? selectedRoute.path.join(' → ')
                          : 'No available route to the gateway'}
                      </p>
                    </div>

                    <button
                      className={
                        selectedSensor.isOnline
                          ? 'mesh-button mesh-button-danger'
                          : 'mesh-button mesh-button-primary'
                      }
                      type="button"
                      disabled={busy}
                      onClick={() =>
                        void handleToggleNode(
                          selectedSensor.id,
                          selectedSensor.isOnline,
                        )
                      }
                    >
                      {selectedSensor.isOnline
                        ? 'Simulate Sensor Failure'
                        : 'Restore Sensor'}
                    </button>
                  </div>
                )}

                <div className="mesh-mst-summary">
                  <h3>Minimum Spanning Tree</h3>
                  <div className="mesh-detail-row">
                    <span>Total link cost</span>
                    <strong>{mst?.totalCost.toFixed(2)}</strong>
                  </div>
                  <div className="mesh-detail-row">
                    <span>Selected links</span>
                    <strong>{mst?.selectedLinks.length}</strong>
                  </div>
                  <div className="mesh-detail-row">
                    <span>Online components</span>
                    <strong>{mst?.onlineComponentCount}</strong>
                  </div>
                  <div className="mesh-detail-row">
                    <span>All online nodes connected</span>
                    <strong>{mst?.spansAllOnlineNodes ? 'Yes' : 'No'}</strong>
                  </div>
                </div>
              </aside>
            </section>

            <section className="mesh-bottom-grid">
              <div className="mesh-card">
                <h2>Create Wireless Link</h2>
                <p>
                  Define the connections that the MST and routing algorithms
                  are allowed to use.
                </p>

                <form className="mesh-form" onSubmit={event =>
                  void handleAddLink(event)}>
                  <label>
                    From
                    <select
                      value={from}
                      onChange={event => setFrom(event.target.value)}
                    >
                      {topology.nodes.map(node => (
                        <option key={node.id} value={node.id}>
                          {node.label} ({node.id})
                        </option>
                      ))}
                    </select>
                  </label>

                  <label>
                    To
                    <select
                      value={to}
                      onChange={event => setTo(event.target.value)}
                      required
                    >
                      <option value="">Choose destination</option>
                      {topology.nodes.map(node => (
                        <option key={node.id} value={node.id}>
                          {node.label} ({node.id})
                        </option>
                      ))}
                    </select>
                  </label>

                  <label>
                    Link cost
                    <input
                      type="number"
                      min="0.1"
                      step="0.1"
                      value={cost}
                      onChange={event => setCost(Number(event.target.value))}
                      required
                    />
                  </label>

                  <label>
                    Signal strength (dBm)
                    <input
                      type="number"
                      min="-120"
                      max="0"
                      value={rssi}
                      onChange={event => setRssi(Number(event.target.value))}
                      required
                    />
                  </label>

                  <button
                    className="mesh-button mesh-button-primary"
                    type="submit"
                    disabled={busy || !to || from === to}
                  >
                    Add Wireless Link
                  </button>
                </form>
              </div>

              <div className="mesh-card">
                <h2>Active Connections</h2>
                <p>
                  Remove a link to simulate a wireless connection failure.
                </p>

                <div className="mesh-link-list">
                  {topology.links.map(link => (
                    <div
                      className="mesh-link-item"
                      key={linkKey(link.from, link.to)}
                    >
                      <div>
                        <strong>{link.from} ↔ {link.to}</strong>
                        <span>
                          Cost: {link.cost.toFixed(2)} |
                          Signal: {link.rssiDbm} dBm
                        </span>
                      </div>
                      <button
                        type="button"
                        disabled={busy}
                        onClick={() => void handleRemoveLink(link)}
                      >
                        Remove
                      </button>
                    </div>
                  ))}

                  {topology.links.length === 0 && (
                    <div className="mesh-empty">
                      No wireless connections configured yet.
                    </div>
                  )}
                </div>
              </div>
            </section>
          </main>
        )}
      </div>
    </div>
  )
}

export default MeshTopologyPage
