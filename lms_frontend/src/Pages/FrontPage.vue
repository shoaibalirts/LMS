<template>
  <div class="page-shell">
    <section class="hero">
      <div class="hero-content">
        <p class="badge">LMS Portal</p>
        <h1>Learning Management System</h1>
        <p class="lede">
          A comprehensive platform for managing courses, tracking progress, and empowering both teachers and students.
        </p>

        <div class="cta-buttons">
          <router-link to="/login" class="btn btn-primary">
            <span class="btn-icon">👨‍🏫</span>
            Teacher Login
          </router-link>
          <router-link to="/student-login" class="btn btn-secondary">
            <span class="btn-icon">👨‍🎓</span>
            Student Login
          </router-link>
        </div>

        <div class="features">
          <div class="feature">
            <div class="feature-icon">📚</div>
            <div>
              <h3>Course Management</h3>
              <p>Create and organize courses with ease</p>
            </div>
          </div>
          <div class="feature">
            <div class="feature-icon">📊</div>
            <div>
              <h3>Progress Tracking</h3>
              <p>Monitor student performance in real-time</p>
            </div>
          </div>
          <div class="feature">
            <div class="feature-icon">💬</div>
            <div>
              <h3>Communication</h3>
              <p>Stay connected with announcements</p>
            </div>
          </div>
        </div>
      </div>

      <div class="hero-visual">
        <div class="visual-card">
          <div class="visual-header">
            <div class="visual-dots">
              <span></span>
              <span></span>
              <span></span>
            </div>
          </div>
          <div class="visual-content">
            <div class="visual-line short"></div>
            <div class="visual-line"></div>
            <div class="visual-line medium"></div>
            <div class="visual-grid">
              <div class="visual-box"></div>
              <div class="visual-box"></div>
              <div class="visual-box"></div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
</script>

<style scoped>
.page-shell {
  width: 100%;
  height: calc(100vh - 52px);
  padding: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background: radial-gradient(circle at 20% 20%, rgba(58, 123, 213, 0.15), transparent 35%),
    radial-gradient(circle at 80% 0%, rgba(255, 166, 43, 0.18), transparent 30%),
    linear-gradient(135deg, #0f172a 0%, #0b1324 45%, #0f172a 100%);
}

.hero {
  width: min(1200px, 100%);
  display: grid;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 3rem;
  align-items: center;
}

.hero-content {
  color: #e2e8f0;
  padding: 1rem;
}

.badge {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 999px;
  font-size: 0.85rem;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  margin-bottom: 1rem;
}

h1 {
  font-size: clamp(2.2rem, 4vw, 3rem);
  margin: 0.8rem 0;
  color: #f8fafc;
  line-height: 1.2;
}

.lede {
  color: #cbd5e1;
  font-size: 1.1rem;
  max-width: 50ch;
  line-height: 1.7;
  margin-bottom: 2rem;
}

.cta-buttons {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  margin-bottom: 3rem;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.6rem;
  padding: 1rem 1.8rem;
  border-radius: 14px;
  font-weight: 700;
  font-size: 1rem;
  text-decoration: none;
  transition: transform 0.15s ease, box-shadow 0.15s ease, filter 0.15s ease;
}

.btn-icon {
  font-size: 1.2rem;
}

.btn-primary {
  background: linear-gradient(135deg, #22d3ee, #6366f1);
  color: #0b1222;
  box-shadow: 0 12px 30px rgba(99, 102, 241, 0.35);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 16px 36px rgba(99, 102, 241, 0.45);
  filter: brightness(1.05);
}

.btn-secondary {
  background: rgba(255, 255, 255, 0.08);
  color: #e2e8f0;
  border: 1px solid rgba(255, 255, 255, 0.15);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.2);
}

.btn-secondary:hover {
  transform: translateY(-2px);
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(255, 255, 255, 0.25);
}

.features {
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
}

.feature {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 0.5rem 0;
}

.feature-icon {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(34, 211, 238, 0.1);
  border: 1px solid rgba(34, 211, 238, 0.2);
  border-radius: 12px;
  font-size: 1.3rem;
}

.feature h3 {
  margin: 0 0 0.25rem 0;
  font-size: 1rem;
  color: #f8fafc;
}

.feature p {
  margin: 0;
  color: #94a3b8;
  font-size: 0.9rem;
}

.hero-visual {
  display: flex;
  justify-content: center;
  align-items: center;
}

.visual-card {
  width: 100%;
  max-width: 380px;
  background: #0b1222;
  border-radius: 18px;
  box-shadow: 0 25px 60px rgba(0, 0, 0, 0.5), 0 0 0 1px rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.05);
  overflow: hidden;
}

.visual-header {
  padding: 1rem 1.2rem;
  background: rgba(255, 255, 255, 0.03);
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.visual-dots {
  display: flex;
  gap: 6px;
}

.visual-dots span {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: #334155;
}

.visual-dots span:first-child {
  background: #fb7185;
}

.visual-dots span:nth-child(2) {
  background: #fbbf24;
}

.visual-dots span:nth-child(3) {
  background: #4ade80;
}

.visual-content {
  padding: 1.5rem;
}

.visual-line {
  height: 12px;
  background: linear-gradient(90deg, rgba(34, 211, 238, 0.3), rgba(99, 102, 241, 0.3));
  border-radius: 6px;
  margin-bottom: 0.8rem;
}

.visual-line.short {
  width: 40%;
}

.visual-line.medium {
  width: 70%;
}

.visual-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.8rem;
  margin-top: 1.5rem;
}

.visual-box {
  aspect-ratio: 1;
  background: rgba(255, 255, 255, 0.04);
  border: 1px dashed rgba(255, 255, 255, 0.1);
  border-radius: 10px;
}

@media (max-width: 960px) {
  .hero {
    grid-template-columns: 1fr;
    text-align: center;
  }

  .hero-content {
    order: 1;
  }

  .hero-visual {
    order: 2;
  }

  .lede {
    max-width: none;
  }

  .cta-buttons {
    justify-content: center;
  }

  .features {
    align-items: center;
  }

  .feature {
    max-width: 300px;
  }
}

@media (max-width: 560px) {
  .cta-buttons {
    flex-direction: column;
  }

  .btn {
    width: 100%;
    justify-content: center;
  }
}
</style>
