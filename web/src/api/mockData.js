/**
 * Aether.Admin 企业级后台基础数据与演示数据
 * 完全遵循 .NET 10 后端实体结构与 RBAC 规范
 */

export const mockTenants = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    name: '默认租户 (Default)',
    code: 'default',
    status: true,
    expireTime: '2030-12-31T23:59:59Z',
    permissionVersion: 1,
    createdAt: '2026-01-01T08:00:00Z',
    updatedAt: '2026-01-01T08:00:00Z'
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    name: '华东区域子公司',
    code: 'tenant_east',
    status: true,
    expireTime: '2028-06-30T23:59:59Z',
    permissionVersion: 1,
    createdAt: '2026-02-15T10:30:00Z',
    updatedAt: '2026-02-15T10:30:00Z'
  },
  {
    id: '33333333-3333-3333-3333-333333333333',
    name: '国际业务事业群',
    code: 'tenant_global',
    status: false,
    expireTime: '2026-10-01T00:00:00Z',
    permissionVersion: 1,
    createdAt: '2026-03-20T14:00:00Z',
    updatedAt: '2026-03-20T14:00:00Z'
  }
]

export const mockOrganizations = [
  {
    id: 'org-001',
    name: 'Aether 集团总部',
    code: 'HQ',
    status: true,
    sort: 1,
    version: 1
  },
  {
    id: 'org-002',
    name: '云原生研发中心',
    code: 'RND',
    status: true,
    sort: 2,
    version: 1
  }
]

export const mockDepartments = [
  {
    id: 'd0000000-0000-0000-0000-000000000001',
    name: 'Aether 集团控股',
    organizationId: 'org-001',
    parentId: null,
    treePath: '/1',
    leaderId: 'u0000000-0000-0000-0000-000000000001',
    sort: 1,
    status: true,
    version: 1,
    children: [
      {
        id: 'd0000000-0000-0000-0000-000000000002',
        name: '研发总院',
        organizationId: 'org-001',
        parentId: 'd0000000-0000-0000-0000-000000000001',
        treePath: '/1/2',
        leaderId: 'u0000000-0000-0000-0000-000000000001',
        sort: 1,
        status: true,
        version: 1,
        children: [
          {
            id: 'd0000000-0000-0000-0000-000000000003',
            name: '基础架构部',
            organizationId: 'org-001',
            parentId: 'd0000000-0000-0000-0000-000000000002',
            treePath: '/1/2/3',
            leaderId: null,
            sort: 1,
            status: true,
            version: 1,
            children: []
          },
          {
            id: 'd0000000-0000-0000-0000-000000000004',
            name: '应用开发部',
            organizationId: 'org-001',
            parentId: 'd0000000-0000-0000-0000-000000000002',
            treePath: '/1/2/4',
            leaderId: null,
            sort: 2,
            status: true,
            version: 1,
            children: []
          }
        ]
      },
      {
        id: 'd0000000-0000-0000-0000-000000000005',
        name: '运营中心',
        organizationId: 'org-001',
        parentId: 'd0000000-0000-0000-0000-000000000001',
        treePath: '/1/5',
        leaderId: null,
        sort: 2,
        status: true,
        version: 1,
        children: []
      }
    ]
  }
]

export const mockPosts = [
  {
    id: 'p0000000-0000-0000-0000-000000000001',
    name: '首席架构师',
    code: 'CHIEF_ARCHITECT',
    sort: 1,
    status: true,
    version: 1
  },
  {
    id: 'p0000000-0000-0000-0000-000000000002',
    name: '高级后端工程师',
    code: 'SR_BACKEND_ENG',
    sort: 2,
    status: true,
    version: 1
  },
  {
    id: 'p0000000-0000-0000-0000-000000000003',
    name: '高级前端工程师',
    code: 'SR_FRONTEND_ENG',
    sort: 3,
    status: true,
    version: 1
  },
  {
    id: 'p0000000-0000-0000-0000-000000000004',
    name: '运维研发工程师 (DevOps)',
    code: 'DEVOPS_ENG',
    sort: 4,
    status: true,
    version: 1
  }
]

export const mockRoles = [
  {
    id: 'r0000000-0000-0000-0000-000000000001',
    name: '超级管理员',
    code: 'admin',
    type: 1, // System
    status: true,
    version: 1
  },
  {
    id: 'r0000000-0000-0000-0000-000000000002',
    name: '安全审计员',
    code: 'auditor',
    type: 2, // Custom
    status: true,
    version: 1
  },
  {
    id: 'r0000000-0000-0000-0000-000000000003',
    name: '部门主管',
    code: 'dept_manager',
    type: 2,
    status: true,
    version: 1
  }
]

/**
 * 完整权限树与动态菜单
 * Type: 1 = Directory, 2 = Menu, 3 = Button
 */
export const mockMenus = [
  {
    id: 'm-dash',
    name: '仪表盘',
    type: 2,
    parentId: null,
    path: '/dashboard',
    component: 'dashboard/index',
    permissionCode: null,
    icon: 'DashboardOutlined',
    sort: 1,
    visible: true,
    status: true,
    version: 1,
    children: []
  },
  {
    id: 'm-identity',
    name: '用户中心',
    type: 1,
    parentId: null,
    path: '/identity',
    component: 'BasicLayout',
    permissionCode: 'system:user:list',
    icon: 'UserOutlined',
    sort: 2,
    visible: true,
    status: true,
    version: 1,
    children: [
      {
        id: 'm-user',
        name: '用户管理',
        type: 2,
        parentId: 'm-identity',
        path: '/identity/user',
        component: 'identity/user/index',
        permissionCode: 'system:user:list',
        icon: 'TeamOutlined',
        sort: 1,
        visible: true,
        status: true,
        version: 1,
        children: [
          { id: 'btn-user-create', name: '新增用户', type: 3, parentId: 'm-user', permissionCode: 'system:user:create', sort: 1, visible: false, status: true },
          { id: 'btn-user-update', name: '编辑用户', type: 3, parentId: 'm-user', permissionCode: 'system:user:update', sort: 2, visible: false, status: true },
          { id: 'btn-user-delete', name: '删除用户', type: 3, parentId: 'm-user', permissionCode: 'system:user:delete', sort: 3, visible: false, status: true }
        ]
      }
    ]
  },
  {
    id: 'm-org',
    name: '组织架构',
    type: 1,
    parentId: null,
    path: '/organization',
    component: 'BasicLayout',
    permissionCode: 'system:dept:list',
    icon: 'ApartmentOutlined',
    sort: 3,
    visible: true,
    status: true,
    version: 1,
    children: [
      {
        id: 'm-dept',
        name: '部门管理',
        type: 2,
        parentId: 'm-org',
        path: '/organization/dept',
        component: 'organization/dept/index',
        permissionCode: 'system:dept:list',
        icon: 'ClusterOutlined',
        sort: 1,
        visible: true,
        status: true,
        version: 1,
        children: [
          { id: 'btn-dept-create', name: '新增部门', type: 3, parentId: 'm-dept', permissionCode: 'system:dept:create', sort: 1, visible: false, status: true },
          { id: 'btn-dept-update', name: '编辑部门', type: 3, parentId: 'm-dept', permissionCode: 'system:dept:update', sort: 2, visible: false, status: true },
          { id: 'btn-dept-delete', name: '删除部门', type: 3, parentId: 'm-dept', permissionCode: 'system:dept:delete', sort: 3, visible: false, status: true }
        ]
      },
      {
        id: 'm-post',
        name: '岗位管理',
        type: 2,
        parentId: 'm-org',
        path: '/organization/post',
        component: 'organization/post/index',
        permissionCode: 'system:post:list',
        icon: 'IdcardOutlined',
        sort: 2,
        visible: true,
        status: true,
        version: 1,
        children: [
          { id: 'btn-post-create', name: '新增岗位', type: 3, parentId: 'm-post', permissionCode: 'system:post:create', sort: 1, visible: false, status: true },
          { id: 'btn-post-update', name: '编辑岗位', type: 3, parentId: 'm-post', permissionCode: 'system:post:update', sort: 2, visible: false, status: true },
          { id: 'btn-post-delete', name: '删除岗位', type: 3, parentId: 'm-post', permissionCode: 'system:post:delete', sort: 3, visible: false, status: true }
        ]
      }
    ]
  },
  {
    id: 'm-auth',
    name: '权限中心',
    type: 1,
    parentId: null,
    path: '/authorization',
    component: 'BasicLayout',
    permissionCode: 'system:role:list',
    icon: 'SafetyCertificateOutlined',
    sort: 4,
    visible: true,
    status: true,
    version: 1,
    children: [
      {
        id: 'm-role',
        name: '角色管理',
        type: 2,
        parentId: 'm-auth',
        path: '/authorization/role',
        component: 'authorization/role/index',
        permissionCode: 'system:role:list',
        icon: 'UserSwitchOutlined',
        sort: 1,
        visible: true,
        status: true,
        version: 1,
        children: [
          { id: 'btn-role-create', name: '新增角色', type: 3, parentId: 'm-role', permissionCode: 'system:role:create', sort: 1, visible: false, status: true },
          { id: 'btn-role-update', name: '编辑角色', type: 3, parentId: 'm-role', permissionCode: 'system:role:update', sort: 2, visible: false, status: true },
          { id: 'btn-role-delete', name: '删除角色', type: 3, parentId: 'm-role', permissionCode: 'system:role:delete', sort: 3, visible: false, status: true },
          { id: 'btn-role-assign', name: '分配权限', type: 3, parentId: 'm-role', permissionCode: 'system:permission:assign', sort: 4, visible: false, status: true }
        ]
      },
      {
        id: 'm-menu-mgr',
        name: '菜单管理',
        type: 2,
        parentId: 'm-auth',
        path: '/authorization/menu',
        component: 'authorization/menu/index',
        permissionCode: 'system:menu:list',
        icon: 'MenuOutlined',
        sort: 2,
        visible: true,
        status: true,
        version: 1,
        children: [
          { id: 'btn-menu-create', name: '新增菜单', type: 3, parentId: 'm-menu-mgr', permissionCode: 'system:menu:create', sort: 1, visible: false, status: true },
          { id: 'btn-menu-update', name: '编辑菜单', type: 3, parentId: 'm-menu-mgr', permissionCode: 'system:menu:update', sort: 2, visible: false, status: true },
          { id: 'btn-menu-delete', name: '删除菜单', type: 3, parentId: 'm-menu-mgr', permissionCode: 'system:menu:delete', sort: 3, visible: false, status: true }
        ]
      }
    ]
  },
  {
    id: 'm-tenant',
    name: '租户管理',
    type: 2,
    parentId: null,
    path: '/tenant/list',
    component: 'tenant/index',
    permissionCode: 'system:tenant:list',
    icon: 'CloudServerOutlined',
    sort: 5,
    visible: true,
    status: true,
    version: 1,
    children: [
      { id: 'btn-tenant-create', name: '新增租户', type: 3, parentId: 'm-tenant', permissionCode: 'system:tenant:create', sort: 1, visible: false, status: true },
      { id: 'btn-tenant-update', name: '编辑租户', type: 3, parentId: 'm-tenant', permissionCode: 'system:tenant:update', sort: 2, visible: false, status: true }
    ]
  },
  {
    id: 'm-audit',
    name: '审计管理',
    type: 2,
    parentId: null,
    path: '/audit/log',
    component: 'audit/index',
    permissionCode: 'system:audit:list',
    icon: 'HistoryOutlined',
    sort: 6,
    visible: true,
    status: true,
    version: 1,
    children: []
  }
]

export const mockUsers = [
  {
    id: 'u0000000-0000-0000-0000-000000000001',
    username: 'admin',
    nickname: '超级系统管理员',
    deptId: 'd0000000-0000-0000-0000-000000000003',
    email: 'admin@aether.io',
    mobile: '13800138000',
    status: true,
    version: 1,
    roleIds: ['r0000000-0000-0000-0000-000000000001'],
    postIds: ['p0000000-0000-0000-0000-000000000001'],
    createdAt: '2026-01-01T08:00:00Z'
  },
  {
    id: 'u0000000-0000-0000-0000-000000000002',
    username: 'dev_lead',
    nickname: '李晨 (架构技术主管)',
    deptId: 'd0000000-0000-0000-0000-000000000004',
    email: 'lichen@aether.io',
    mobile: '13912345678',
    status: true,
    version: 1,
    roleIds: ['r0000000-0000-0000-0000-000000000003'],
    postIds: ['p0000000-0000-0000-0000-000000000002'],
    createdAt: '2026-02-10T09:30:00Z'
  },
  {
    id: 'u0000000-0000-0000-0000-000000000003',
    username: 'auditor_zhang',
    nickname: '张宁 (合规审计专员)',
    deptId: 'd0000000-0000-0000-0000-000000000005',
    email: 'audit@aether.io',
    mobile: '13798765432',
    status: true,
    version: 1,
    roleIds: ['r0000000-0000-0000-0000-000000000002'],
    postIds: ['p0000000-0000-0000-0000-000000000004'],
    createdAt: '2026-03-01T11:20:00Z'
  }
]

export const mockPermissions = [
  'system:user:list', 'system:user:create', 'system:user:update', 'system:user:delete',
  'system:dept:list', 'system:dept:create', 'system:dept:update', 'system:dept:delete',
  'system:organization:list', 'system:organization:create', 'system:organization:update', 'system:organization:delete',
  'system:post:list', 'system:post:create', 'system:post:update', 'system:post:delete',
  'system:role:list', 'system:role:create', 'system:role:update', 'system:role:delete',
  'system:menu:list', 'system:menu:create', 'system:menu:update', 'system:menu:delete',
  'system:permission:assign', 'system:audit:list', 'system:tenant:list', 'system:tenant:create', 'system:tenant:update'
]

export const mockAuditLogs = [
  {
    id: 'a0000000-0000-0000-0000-000000000001',
    userId: 'u0000000-0000-0000-0000-000000000001',
    method: 'POST',
    endpoint: '/api/auth/login',
    parameters: '{"tenantCode":"default","username":"admin"}',
    ip: '127.0.0.1',
    traceId: 'trace-88fa3910bce1',
    result: 200,
    durationMs: 42,
    createdAt: '2026-10-10T14:35:10Z'
  },
  {
    id: 'a0000000-0000-0000-0000-000000000002',
    userId: 'u0000000-0000-0000-0000-000000000001',
    method: 'GET',
    endpoint: '/api/users',
    parameters: '{"page":1,"size":20}',
    ip: '127.0.0.1',
    traceId: 'trace-88fa3910bce2',
    result: 200,
    durationMs: 18,
    createdAt: '2026-10-10T14:35:12Z'
  },
  {
    id: 'a0000000-0000-0000-0000-000000000003',
    userId: 'u0000000-0000-0000-0000-000000000001',
    method: 'PUT',
    endpoint: '/api/roles/r0000000-0000-0000-0000-000000000001/permissions',
    parameters: '{"scopeType":1,"menuIds":[...]}',
    ip: '192.168.1.102',
    traceId: 'trace-88fa3910bce3',
    result: 200,
    durationMs: 65,
    createdAt: '2026-10-10T14:36:01Z'
  },
  {
    id: 'a0000000-0000-0000-0000-000000000004',
    userId: 'u0000000-0000-0000-0000-000000000002',
    method: 'POST',
    endpoint: '/api/posts',
    parameters: '{"name":"AI应用工程师","code":"AI_ENG"}',
    ip: '192.168.1.150',
    traceId: 'trace-88fa3910bce4',
    result: 200,
    durationMs: 34,
    createdAt: '2026-10-10T14:40:22Z'
  }
]

