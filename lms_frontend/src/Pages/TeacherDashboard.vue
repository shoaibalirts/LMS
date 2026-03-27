<template>
  <div class="dashboard">
    <div class="dashboard-header">
      <div>
        <p class="badge">Teacher Portal</p>
        <h1>Welcome to your Dashboard</h1>
        <p class="subtitle">Manage your courses, students, and educational content</p>
      </div>
    </div>

    <div class="dashboard-grid">
      <div class="stat-card">
        <div class="stat-icon courses-icon">📚</div>
        <div class="stat-content">
          <h3>Courses</h3>
          <p class="stat-number">0</p>
          <p class="stat-label">Active courses</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon students-icon">👨‍🎓</div>
        <div class="stat-content">
          <h3>Students</h3>
          <p class="stat-number">0</p>
          <p class="stat-label">Enrolled students</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon assignments-icon">📝</div>
        <div class="stat-content">
          <h3>Assignments</h3>
          <p class="stat-number">0</p>
          <p class="stat-label">Pending reviews</p>
        </div>
      </div>
    </div>

    <div class="dashboard-content">
      <div class="content-card">
        <h2>Recent Activity</h2>
        <div class="empty-state">
          <p>No recent activity</p>
          <p class="muted">Your recent actions will appear here</p>
        </div>
      </div>

      <div class="content-card">
        <h2>Quick Actions</h2>
        <div class="quick-actions">
          <router-link to="/create-task" class="action-btn">
            <span>📝</span> Create Task
          </router-link>
          <button class="action-btn">
            <span>📚</span> Create Course
          </button>
          <button class="action-btn">
            <span>👥</span> Manage Students
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
</script>

<style scoped>
.dashboard {
  height: calc(100vh - 52px);
  background: #f8fafc;
  padding: 2rem;
  overflow: hidden;
}

.dashboard-header {
  max-width: 1200px;
  margin: 0 auto 2rem;
  padding: 2rem;
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
  border-radius: 18px;
  color: #f8fafc;
}

.badge {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  background: rgba(34, 211, 238, 0.15);
  border: 1px solid rgba(34, 211, 238, 0.25);
  border-radius: 999px;
  font-size: 0.85rem;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: #22d3ee;
  margin-bottom: 0.5rem;
}

.dashboard-header h1 {
  font-size: clamp(1.8rem, 3vw, 2.4rem);
  margin: 0.5rem 0;
}

.subtitle {
  color: #94a3b8;
  font-size: 1rem;
  margin: 0;
}

.dashboard-grid {
  max-width: 1200px;
  margin: 0 auto 2rem;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
}

.stat-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1.2rem;
  box-shadow: 0 4px 20px rgba(15, 23, 42, 0.06);
  border: 1px solid rgba(15, 23, 42, 0.06);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 30px rgba(15, 23, 42, 0.1);
}

.stat-icon {
  width: 56px;
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  font-size: 1.5rem;
}

.courses-icon {
  background: rgba(99, 102, 241, 0.1);
}

.students-icon {
  background: rgba(34, 211, 238, 0.1);
}

.assignments-icon {
  background: rgba(251, 191, 36, 0.1);
}

.stat-content h3 {
  margin: 0;
  font-size: 0.9rem;
  color: #64748b;
  font-weight: 500;
}

.stat-number {
  margin: 0.25rem 0;
  font-size: 2rem;
  font-weight: 700;
  color: #0f172a;
}

.stat-label {
  margin: 0;
  font-size: 0.85rem;
  color: #94a3b8;
}

.dashboard-content {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.content-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(15, 23, 42, 0.06);
  border: 1px solid rgba(15, 23, 42, 0.06);
}

.content-card h2 {
  margin: 0 0 1.2rem;
  font-size: 1.2rem;
  color: #0f172a;
}

.empty-state {
  padding: 2rem;
  text-align: center;
  background: #f8fafc;
  border-radius: 12px;
  border: 1px dashed rgba(15, 23, 42, 0.12);
}

.empty-state p {
  margin: 0.25rem 0;
  color: #475569;
}

.muted {
  color: #94a3b8 !important;
  font-size: 0.9rem;
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  background: #f8fafc;
  border: 1px solid rgba(15, 23, 42, 0.08);
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 500;
  color: #0f172a;
  cursor: pointer;
  transition: background 0.15s ease, border-color 0.15s ease;
  text-decoration: none;
}

.action-btn:hover {
  background: #f1f5f9;
  border-color: rgba(34, 211, 238, 0.3);
}

.action-btn span {
  font-size: 1.1rem;
}

@media (max-width: 768px) {
  .dashboard {
    padding: 1rem;
  }

  .dashboard-content {
    grid-template-columns: 1fr;
  }

  .dashboard-header {
    padding: 1.5rem;
  }
}
</style>
