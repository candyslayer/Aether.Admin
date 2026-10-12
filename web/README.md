# Aether.Admin.Web 企业级后台管理系统

> **Aether.Admin** 是一套面向企业数字化底座的高性能微内核应用基础平台。  
> 前端项目 **Aether.Admin.Web** 采用纯 JavaScript 编写，深度对齐 **.NET 10 (ASP.NET Core Minimal API / EF Core / PostgreSQL / Redis / JWT)** 企业后端，设计参考 **芋道源码 (Yudao Admin)**、**RuoYi-Vue-Pro** 与 **Ant Design Pro**。

---

## 🚀 技术栈特性

- **前端核心**: Vue 3 (`<script setup>` + Composition API)
- **开发构建**: Vite 6.x (极速毫秒级热更新，原生 ESM)
- **开发语言**: **100% 纯 JavaScript**（严格遵循规范，不包含任何 TypeScript、`.ts` 文件或类型注解）
- **UI 组件库**: Ant Design Vue 4.x (Ant Design Pro 设计语言规范)
- **全局状态**: Pinia 2.x (模块化 Store: `user`, `permission`, `tenant`, `app`)
- **前端路由**: Vue Router 4.x (动态路由加载、路由守卫、RBAC 细粒度拦截)
- **网络通信**: Axios (自动注入 JWT Bearer、`X-Tenant-Code`、401/403/409/429/500 统一熔断拦截)
- **数据可视化**: ECharts 5.x (响应式自适应大屏图表、吞吐量趋势、热度分布)
- **辅助库**: Dayjs, NProgress, @ant-design/icons-vue

---

## 📁 项目目录结构

```text
src/
├── api/                   # API 接口层 (按领域模块化划分)
│   ├── request.js         # Axios 封装 (JWT 自动注入、多租户 Header、统一错误处理)
│   ├── auth.js            # 登录、动态菜单树、权限码获取
│   ├── user.js            # 用户管理 CRUD 分页接口
│   ├── role.js            # 角色管理 CRUD 与权限分配接口
│   ├── menu.js            # 菜单树 CRUD 接口
│   ├── dept.js            # 组织部门树与机构管理接口
│   ├── post.js            # 岗位管理 CRUD 接口
│   ├── tenant.js          # 多租户管理接口
│   ├── audit.js           # 审计日志接口
│   ├── dashboard.js       # 仪表盘统计分析数据
│   └── mockData.js        # 企业内置沙盒与容灾演示数据源
│
├── layouts/               # 全局布局架构
│   ├── BasicLayout.vue    # Ant Design Pro 主布局 (Sider, Header, Content, Theme)
│   └── components/
│       ├── HeaderBar.vue  # 顶部栏 (租户切换、暗黑模式切换、通知中心、用户头像、退出)
│       ├── DynamicMenu.vue# RBAC 动态递归菜单 (支持目录、菜单、图标动态渲染)
│       └── RouteTabs.vue  # 多标签页 Tabs (刷新、关闭当前、关闭其它、关闭所有)
│
├── router/                # 路由中心
│   ├── index.js           # 静态路由与路由实例定义
│   └── permission.js      # 路由守卫 (NProgress 进度条、登录校验、动态权限路由注入)
│
├── stores/                # Pinia 状态管理
│   ├── user.js            # Token、用户信息、角色、权限码列表
│   ├── permission.js      # 动态路由解析器与菜单树映射
│   ├── tenant.js          # 多租户状态与切换调度
│   └── app.js             # 主题模式 (Light/Dark)、主色调、折叠状态、Tabs 标签页
│
├── views/                 # 业务视图页面
│   ├── login/             # 登录模块 (租户选择、用户名、密码、预设快捷填充)
│   ├── dashboard/         # 仪表盘首页 (指标统计卡片、ECharts 访问/日志趋势图、快捷入口)
│   ├── identity/          # 用户中心
│   │   └── user/          # 用户管理 (搜索、表格分页、部门树选择、岗位角色分配)
│   ├── organization/      # 组织架构
│   │   ├── dept/          # 部门管理 (树形表格、级联新增、负责人、状态)
│   │   └── post/          # 岗位管理 (编码、排序、状态 CRUD)
│   ├── authorization/     # 权限中心
│   │   ├── role/          # 角色管理 (CRUD、Tree 组件权限勾选分配、数据范围 Scope)
│   │   └── menu/          # 菜单管理 (树形表格、目录/菜单/按钮三级类型维护)
│   ├── tenant/            # 租户管理 (租户列表、租约有效期、状态控制)
│   ├── audit/             # 审计日志 (HTTP 请求方法、IP、耗时、分布式 TraceId、Payload Drawer)
│   └── error/             # 错误状态页 (403 权限拒绝、404 未找到)
│
├── components/            # 通用业务组件
│   ├── Permission/        # 声明式权限控制组件 `<Permission value="...">`
│   ├── TreeSelect/        # 部门/上级节点通用树选择器
│   ├── Table/             # 企业级通用表格容器
│   └── DynamicIcon.vue    # 动态 Ant Design 图标渲染组件
│
├── directives/            # 自定义指令
│   └── permission.js      # 按钮权限指令 `v-permission`
│
├── utils/                 # 工具库
│   ├── token.js           # LocalStorage Token 与租户存储
│   ├── tree.js            # 树结构递归构建、平铺、查找、过滤通用算法
│   └── format.js          # 时间日期、耗时与字节格式化
│
├── App.vue                # 根组件 (Antd zh_CN 国际化与主题算法配置)
└── main.js                # 应用启动入口
```

---

## 🔐 核心业务与 RBAC 权限设计

### 1. 权限流转模型
$$\text{User} \longrightarrow \text{Role} \longrightarrow \text{Menu} \longrightarrow \text{Permission}$$
- **用户登录**: 调用 `POST /api/auth/login`，获取 `accessToken`、`roles`、`permissions`。
- **动态菜单**: 调用 `GET /api/auth/menus`，后端返回该用户权限下的树形节点，前端动态构建路由并挂载至 `Vue Router`。
- **按钮级别权限**:
  - **指令方式**: `<a-button v-permission="'system:user:create'">新增用户</a-button>`
  - **组件方式**: `<Permission value="'system:user:delete'"><a-button danger>删除</a-button></Permission>`
  - 若无权限，元素将被自动安全移除。

### 2. 多租户架构支持
- 顶部导航栏提供**租户即时切换**下拉菜单。
- 登录页面支持租户选择与默认代码 (`default` / `tenant_east` / `tenant_global`)。
- Axios 自动在每次 HTTP 请求的 Header 中附加 `X-Tenant-Code`，支持逻辑与物理多租户隔离。

### 3. 主题与界面
- **Light / Dark 模式一键切换**: 采用 Ant Design Vue 4 `theme.darkAlgorithm` 与 `theme.defaultAlgorithm` 动态算法。
- **多标签页 (Tabs)**: 支持路由即时缓存、双击/关闭、关闭其他、关闭所有及单页面刷新。

---

## 🛠️ 快速启动

### 1. 安装依赖
```bash
npm install
```

### 2. 本地开发
```bash
npm run dev
```
启动后访问: `http://localhost:3000`

### 3. 生产构建
```bash
npm run build
```

### 4. 构建预览
```bash
npm run preview
```

---

## ⚙️ 环境配置说明

- `.env.development`: 开发环境，API 地址配置为 `http://127.0.0.1:5072`，启用沙盒回退开关。
- `.env.production`: 生产环境，API 地址配置为 `/api`，配合 Nginx 反向代理。

```env
# .env.development
VITE_APP_TITLE=Aether.Admin 企业管理平台
VITE_API_BASE_URL=http://127.0.0.1:5072
VITE_USE_MOCK_FALLBACK=true
```

---

## 📜 默认演示账号

| 账号身份 | 用户名 | 默认密码 | 租户代码 | 权限说明 |
| :--- | :--- | :--- | :--- | :--- |
| **超级管理员** | `admin` | `Password123456!` | `default` | 拥有全系统所有菜单与按钮操作权限 |
| **架构技术主管** | `dev_lead` | `Password123456!` | `default` | 研发团队部门与岗位管理权限 |

