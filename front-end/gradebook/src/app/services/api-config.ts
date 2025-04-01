// API Configuration
export const API_CONFIG = {
  BASE_URL: 'https://localhost:7203/api',
  
  // Auth endpoints
  AUTH: {
    REGISTER: '/Auth/register',
    LOGIN: '/Auth/login',
    PROFILE: '/Auth/profile',
    UPDATE_PROFILE: '/Auth/profile',
    CHANGE_PASSWORD: '/Auth/password'
  },
  
  // Grade endpoints
  GRADE: {
    BASE: '/Grade',
    STUDENT: '/Grade/student',
    CLASS: '/Grade/class',
    ASSIGNMENT: '/Grade/assignment',
    BATCH: '/Grade/batch'
  },
  
  // Class endpoints
  CLASS: {
    BASE: '/Class',
    STUDENT: '/Class/student'
  },
  
  // Assignment endpoints
  ASSIGNMENT: {
    BASE: '/Assignment',
    CLASS: '/Assignment/assignments/class'
  },
  
  // Student endpoints
  STUDENT: {
    BASE: '/Student'
  },
  
  // Audit Log endpoints
  AUDIT_LOG: {
    BASE: '/AuditLog',
    ENTITY: '/AuditLog/entity'
  }
}; 