'use client';

import { useEffect, useState } from 'react';
import type { Project, ProjectChangeResponse } from '@/types/api';
import {
  getActiveProjects,
  getAllProjects,
  createProject,
  updateProjectStatus,
  submitProjectChange,
  approveProjectChange,
} from '@/lib/api';

export default function ProjectsPage() {
  const [activeProjects, setActiveProjects] = useState<Project[]>([]);
  const [allProjects, setAllProjects] = useState<Project[]>([]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState<string | null>(null);

  // Change request state
  const [showChangeRequest, setShowChangeRequest] = useState(false);
  const [crTitle, setCrTitle] = useState('');
  const [crDescription, setCrDescription] = useState('');
  const [crJustification, setCrJustification] = useState('');
  const [crReplaceId, setCrReplaceId] = useState('');
  const [changeResponse, setChangeResponse] = useState<ProjectChangeResponse | null>(null);

  const load = async () => {
    try {
      setActiveProjects(await getActiveProjects().catch(() => []));
      setAllProjects(await getAllProjects().catch(() => []));
    } catch (e: any) {
      setError(e.message);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await createProject(title, description);
      setTitle('');
      setDescription('');
      load();
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleStatusChange = async (id: string, status: string) => {
    await updateProjectStatus(id, status);
    load();
  };

  const handleChangeRequest = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      const response = await submitProjectChange(crTitle, crDescription, crJustification, crReplaceId);
      setChangeResponse(response);
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleApproveChange = async () => {
    if (!changeResponse) return;
    await approveProjectChange(changeResponse.id);
    setChangeResponse(null);
    setShowChangeRequest(false);
    setCrTitle('');
    setCrDescription('');
    setCrJustification('');
    setCrReplaceId('');
    load();
  };

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold mb-1">Projects</h1>
      <p className="text-muted text-sm mb-6">Maximum 2 active. No exceptions.</p>

      {error && <p className="text-danger text-sm mb-4">{error}</p>}

      {/* Active Projects */}
      <section className="mb-8">
        <h2 className="text-xs text-muted uppercase tracking-widest mb-3">
          ACTIVE ({activeProjects.length}/2)
        </h2>
        {activeProjects.length > 0 ? (
          <div className="space-y-3">
            {activeProjects.map((p) => (
              <div key={p.id} className="card">
                <div className="flex items-start justify-between">
                  <div>
                    <h3 className="font-medium text-white">{p.title}</h3>
                    <p className="text-sm text-muted mt-1">{p.description}</p>
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleStatusChange(p.id, 'Completed')}
                      className="text-xs text-accent border border-accent/30 px-2 py-1 rounded hover:bg-accent/10"
                    >
                      Done
                    </button>
                    <button
                      onClick={() => handleStatusChange(p.id, 'Paused')}
                      className="text-xs text-warning border border-warning/30 px-2 py-1 rounded hover:bg-warning/10"
                    >
                      Pause
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="card border-dashed">
            <p className="text-muted">No active projects.</p>
          </div>
        )}
      </section>

      {/* Add Project */}
      {activeProjects.length < 2 ? (
        <form onSubmit={handleCreate} className="card mb-8">
          <h2 className="text-sm font-medium text-white mb-4">Add Project</h2>
          <div className="mb-4">
            <label className="label">Title</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="input"
              placeholder="Project name"
              required
            />
          </div>
          <div className="mb-4">
            <label className="label">Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="input"
              rows={2}
              placeholder="What are you building/doing?"
              required
            />
          </div>
          <button type="submit" className="btn-primary">Add Project</button>
        </form>
      ) : (
        <div className="card mb-8 border-warning/30">
          <p className="text-warning text-sm mb-3">
            You have 2 active projects. To add a new one, you must justify dropping an existing one.
          </p>
          <button
            onClick={() => setShowChangeRequest(!showChangeRequest)}
            className="btn-secondary text-sm"
          >
            Request Project Change
          </button>

          {showChangeRequest && (
            <form onSubmit={handleChangeRequest} className="mt-4 space-y-4">
              <div>
                <label className="label">New Project Title</label>
                <input
                  type="text"
                  value={crTitle}
                  onChange={(e) => setCrTitle(e.target.value)}
                  className="input"
                  required
                />
              </div>
              <div>
                <label className="label">New Project Description</label>
                <textarea
                  value={crDescription}
                  onChange={(e) => setCrDescription(e.target.value)}
                  className="input"
                  rows={2}
                  required
                />
              </div>
              <div>
                <label className="label">Which project to replace?</label>
                <select
                  value={crReplaceId}
                  onChange={(e) => setCrReplaceId(e.target.value)}
                  className="input"
                  required
                >
                  <option value="">Select...</option>
                  {activeProjects.map((p) => (
                    <option key={p.id} value={p.id}>{p.title}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="label">Justification (min 50 chars)</label>
                <textarea
                  value={crJustification}
                  onChange={(e) => setCrJustification(e.target.value)}
                  className="input"
                  rows={3}
                  placeholder="Why is this switch necessary? Convince the AI gatekeeper."
                  required
                  minLength={50}
                />
                <p className="text-xs text-muted mt-1">{crJustification.length}/50 characters</p>
              </div>
              <button type="submit" className="btn-primary">Submit for Review</button>
            </form>
          )}

          {changeResponse && (
            <div className="mt-4 p-4 bg-surface border border-border rounded-lg">
              <p className="text-sm text-muted uppercase tracking-wide mb-2">AI GATEKEEPER SAYS:</p>
              <p className="text-sm text-white">{changeResponse.aiRecommendation}</p>
              <div className="flex gap-3 mt-4">
                <button onClick={handleApproveChange} className="btn-primary text-sm">
                  Proceed Anyway
                </button>
                <button
                  onClick={() => { setChangeResponse(null); setShowChangeRequest(false); }}
                  className="btn-secondary text-sm"
                >
                  Stay Focused
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* All Projects */}
      {allProjects.filter(p => p.status !== 'Active').length > 0 && (
        <section>
          <h2 className="text-xs text-muted uppercase tracking-widest mb-3">PAST PROJECTS</h2>
          <div className="space-y-2">
            {allProjects.filter(p => p.status !== 'Active').map((p) => (
              <div key={p.id} className="card opacity-50">
                <div className="flex justify-between items-center">
                  <span className="text-sm">{p.title}</span>
                  <span className={`text-xs px-2 py-1 rounded ${
                    p.status === 'Completed' ? 'bg-accent/20 text-accent' :
                    p.status === 'Paused' ? 'bg-warning/20 text-warning' :
                    'bg-danger/20 text-danger'
                  }`}>
                    {p.status}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
