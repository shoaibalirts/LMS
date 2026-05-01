const API_BASE_URL = 'http://localhost:5294/api';
const API_ORIGIN = 'http://localhost:5294';
const AUTH_STORAGE_KEY = 'auth';

export function getAssetUrl(path) {
  if (!path) return null;
  if (path.startsWith('http')) return path;
  return `${API_ORIGIN}${path}`;
}

export function getAuthSession() {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

export function setAuthSession(authResponse) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(authResponse));
}

export function clearAuthSession() {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}

export function getAccessToken() {
  const auth = getAuthSession();
  return auth?.token ?? auth?.Token ?? null;
}

function parseJwtPayload(token) {
  try {
    const base64 = token.split('.')[1];
    if (!base64) return null;

    const normalized = base64.replace(/-/g, '+').replace(/_/g, '/');
    const padded = normalized.padEnd(normalized.length + ((4 - normalized.length % 4) % 4), '=');
    return JSON.parse(atob(padded));
  } catch {
    return null;
  }
}

function getTeacherIdFromToken() {
  const token = getAccessToken();
  if (!token) return null;

  const payload = parseJwtPayload(token);
  const teacherIdRaw = payload?.nameid ?? payload?.sub;
  const teacherId = Number(teacherIdRaw);

  return Number.isInteger(teacherId) ? teacherId : null;
}

function getUserIdFromToken() {
  const token = getAccessToken();
  if (!token) return null;

  const payload = parseJwtPayload(token);
  const userIdRaw = payload?.nameid ?? payload?.sub;
  const userId = Number(userIdRaw);

  return Number.isInteger(userId) ? userId : null;
}

function buildHeaders(extraHeaders = {}, requiresAuth = false, isFormData = false) {
  const headers = isFormData ? {} : { 'Content-Type': 'application/json' };
  Object.assign(headers, extraHeaders);

  if (requiresAuth) {
    const token = getAccessToken();
    if (!token) {
      throw new Error('Missing authentication token. Please log in again.');
    }
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

async function request(path, options = {}, requiresAuth = false) {
  const isFormData = options.body instanceof FormData;
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: buildHeaders(options.headers, requiresAuth, isFormData)
  });

  if (response.status === 401) {
    clearAuthSession();
    throw new Error('Unauthorized. Please log in again.');
  }

  if (!response.ok) {
    const text = await response.text();
    const error = new Error(text || `Request failed (${response.status})`);
    error.status = response.status;
    throw error;
  }

  if (response.status === 204) {
    return null;
  }

  const contentType = response.headers.get('content-type') || '';
  if (contentType.toLowerCase().includes('application/json')) {
    return await response.json();
  }

  return await response.text();
}

export async function RegisterTeacher(firstName, lastName, email, password) {
  return await request('/teacher', {
    method: 'POST',
    body: JSON.stringify({ firstName, lastName, email, password })
  });
}

export async function LoginTeacher(email, password) {
  return await request('/teacher/login', {
    method: 'POST',
    body: JSON.stringify({ email, password })
  });
}

export async function LoginStudent(email, password) {
  return await request('/student/login', {
    method: 'POST',
    body: JSON.stringify({ email, password })
  });
}

export async function CreateAssignment(data) {
  const formData = new FormData();
  formData.append('Points', data.Points);
  formData.append('Type', data.Type);
  formData.append('ClassLevel', data.ClassLevel);
  formData.append('Subject', data.Subject);
  if (data.PictureFile) formData.append('PictureFile', data.PictureFile);
  if (data.VideoUrl) formData.append('VideoUrl', data.VideoUrl);
  if (data.Result) formData.append('Result', data.Result);

  return await request('/assignment', {
    method: 'POST',
    body: formData
  }, true);
}

export async function GetTeacherAssignments() {
  return await request('/assignment/teacher', {
    method: 'GET'
  }, true);
}

export async function CreateAssignmentSet(name) {
  const teacherId = getTeacherIdFromToken();
  const payload = teacherId ? { name, teacherId } : { name };

  return await request('/assignmentset', {
    method: 'POST',
    body: JSON.stringify(payload)
  }, true);
}

export async function GetTeacherAssignmentSets() {
  try {
    return await request('/assignmentset', {
      method: 'GET'
    }, true);
  } catch (error) {
    if ((error?.message || '').toLowerCase().includes('no assignment sets found')) {
      return [];
    }

    if (error?.status === 404) {
      const teacherId = getTeacherIdFromToken();
      if (!teacherId) {
        throw new Error('Could not resolve teacher id from JWT token.');
      }

      return await request(`/assignmentset/teacher/${teacherId}`, {
        method: 'GET'
      }, true);
    }

    throw error;
  }
}

export async function AddAssignmentToAssignmentSet(assignmentSetId, assignmentId) {
  return await request(`/assignmentset/${assignmentSetId}/add-assignment/${assignmentId}`, {
    method: 'POST'
  }, true);
}

export async function RegisterStudent(firstName, lastName, email, password) {
  return await request('/student', {
    method: 'POST',
    body: JSON.stringify({ firstName, lastName, email, password })
  }, true);
}

export async function GetTeacherStudents() {
  return await request('/student/teacher', {
    method: 'GET'
  }, true);
}

export async function GetTeacherStudyClasses() {
  return await request('/studyclass/teacher', {
    method: 'GET'
  }, true);
}

export async function GetStudyClassById(id) {
  return await request(`/studyclass/${id}`, {
    method: 'GET'
  }, true);
}

export async function CreateStudyClass(name) {
  return await request('/studyclass', {
    method: 'POST',
    body: JSON.stringify({ name })
  }, true);
}

export async function AddStudentsToStudyClass(studyClassId, studentIds) {
  return await request('/studyclass', {
    method: 'PUT',
    body: JSON.stringify({ id: studyClassId, studentIds })
  }, true);
}

export async function CreateAssignedAssignmentSet(data) {
  return await request('/assignedassignment/sets', {
    method: 'POST',
    body: JSON.stringify(data)
  }, true);
}

export async function GetStudentAssignedAssignmentSets() {
  return await request('/assignedassignment/student', {
    method: 'GET'
  }, true);
}

export async function GetTeacherAssignedAssignmentSets() {
  return await request('/assignedassignment/teacher', {
    method: 'GET'
  }, true);
}

export async function UploadAssignedAssignmentResult(assignedAssignmentId, file) {
  const token = getAccessToken();
  if (!token) {
    throw new Error('Missing authentication token. Please log in again.');
  }

  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE_URL}/assignedassignment/${assignedAssignmentId}/submit`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`
    },
    body: formData
  });

  if (response.status === 401) {
    clearAuthSession();
    throw new Error('Unauthorized. Please log in again.');
  }

  if (!response.ok) {
    const text = await response.text();
    const error = new Error(text || `Request failed (${response.status})`);
    error.status = response.status;
    throw error;
  }

  return await response.json();
}

export async function UpdateAssignedAssignmentFeedback(assignedAssignmentId, feedback) {
  return await request(`/assignedassignment/${assignedAssignmentId}/feedback`, {
    method: 'PUT',
    body: JSON.stringify({ feedback })
  }, true);
}

export async function DownloadAssignedAssignmentResult(assignedAssignmentId) {
  const token = getAccessToken();
  if (!token) {
    throw new Error('Missing authentication token. Please log in again.');
  }

  const response = await fetch(`${API_BASE_URL}/assignedassignment/${assignedAssignmentId}/result`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  if (response.status === 401) {
    clearAuthSession();
    throw new Error('Unauthorized. Please log in again.');
  }

  if (!response.ok) {
    const text = await response.text();
    const error = new Error(text || `Request failed (${response.status})`);
    error.status = response.status;
    throw error;
  }

  const blob = await response.blob();
  const contentDisposition = response.headers.get('content-disposition') || '';
  const match = /filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i.exec(contentDisposition);
  const fileName = decodeURIComponent(match?.[1] || match?.[2] || `submission-${assignedAssignmentId}.pdf`);

  return { blob, fileName };
}

export { getUserIdFromToken };
