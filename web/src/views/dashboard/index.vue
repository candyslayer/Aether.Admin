<template>
  <div class="dashboard-container">
    <!-- 顶部欢迎栏 -->
    <a-card :bordered="false" class="welcome-card">
      <div class="welcome-content">
        <a-avatar :size="64" class="welcome-avatar">
          {{ userStore.nickname ? userStore.nickname.charAt(0) : 'A' }}
        </a-avatar>
        <div class="welcome-text">
          <h2>早安，{{ userStore.nickname }}！欢迎使用 Aether.Admin 企业级管理平台</h2>
          <p>
            当前组织: Aether 集团总部 · 当前租户:
            <a-tag color="blue">{{ tenantStore.currentTenant.name }}</a-tag>
            · 角色权限:
            <a-tag v-for="r in userStore.roles" :key="r" color="green">{{ r }}</a-tag>
          </p>
        </div>
      </div>
    </a-card>

    <!-- 四项关键指标卡片 -->
    <a-row :gutter="[16, 16]" class="stat-cards">
      <a-col :xs="24" :sm="12" :md="6">
        <a-card :bordered="false" class="stat-card">
          <div class="stat-item">
            <div>
              <div class="stat-label">用户总数</div>
              <div class="stat-value">{{ stats.userCount }}</div>
              <div class="stat-desc">较上周增长 <span class="trend-up">+12.5%</span></div>
            </div>
            <div class="stat-icon icon-blue">
              <TeamOutlined />
            </div>
          </div>
        </a-card>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <a-card :bordered="false" class="stat-card">
          <div class="stat-item">
            <div>
              <div class="stat-label">组织部门</div>
              <div class="stat-value">{{ stats.deptCount }}</div>
              <div class="stat-desc">层级深度 <span class="trend-neutral">3 级体系</span></div>
            </div>
            <div class="stat-icon icon-purple">
              <ApartmentOutlined />
            </div>
          </div>
        </a-card>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <a-card :bordered="false" class="stat-card">
          <div class="stat-item">
            <div>
              <div class="stat-label">系统角色</div>
              <div class="stat-value">{{ stats.roleCount }}</div>
              <div class="stat-desc">RBAC 2.0 <span class="trend-up">精细化授权</span></div>
            </div>
            <div class="stat-icon icon-green">
              <SafetyCertificateOutlined />
            </div>
          </div>
        </a-card>
      </a-col>

      <a-col :xs="24" :sm="12" :md="6">
        <a-card :bordered="false" class="stat-card">
          <div class="stat-item">
            <div>
              <div class="stat-label">租户规模</div>
              <div class="stat-value">{{ stats.tenantCount }}</div>
              <div class="stat-desc">多租户隔离 <span class="trend-up">多租户已就绪</span></div>
            </div>
            <div class="stat-icon icon-orange">
              <CloudServerOutlined />
            </div>
          </div>
        </a-card>
      </a-col>
    </a-row>

    <!-- 图表展示区 -->
    <a-row :gutter="[16, 16]" class="chart-section">
      <!-- 访问与 API 吞吐趋势 -->
      <a-col :xs="24" :lg="16">
        <a-card :bordered="false" title="访问趋势与 API 吞吐量" class="chart-card">
          <div ref="visitChartRef" class="chart-box"></div>
        </a-card>
      </a-col>

      <!-- 操作与权限分布 (环形图) -->
      <a-col :xs="24" :lg="8">
        <a-card :bordered="false" title="系统模块操作热度" class="chart-card">
          <div ref="pieChartRef" class="chart-box"></div>
        </a-card>
      </a-col>
    </a-row>

    <!-- 下方快捷管理与系统日志概览 -->
    <a-row :gutter="[16, 16]" style="margin-top: 16px;">
      <a-col :xs="24" :lg="12">
        <a-card :bordered="false" title="系统运行指标" class="info-card">
          <a-descriptions :column="2" bordered size="small">
            <a-descriptions-item label="后端框架">ASP.NET Core Minimal API (.NET 10)</a-descriptions-item>
            <a-descriptions-item label="核心数据库">PostgreSQL (EF Core 10)</a-descriptions-item>
            <a-descriptions-item label="缓存与锁">Redis 7.2 (AetherRedisState)</a-descriptions-item>
            <a-descriptions-item label="鉴权架构">JWT Bearer + RBAC 细粒度控制</a-descriptions-item>
            <a-descriptions-item label="前端技术栈">Vue 3 + Vite + Pinia + Ant Design</a-descriptions-item>
            <a-descriptions-item label="运行可用率">99.99% (已通过健康检查)</a-descriptions-item>
          </a-descriptions>
        </a-card>
      </a-col>

      <a-col :xs="24" :lg="12">
        <a-card :bordered="false" title="快速导航" class="info-card">
          <div class="quick-nav-grid">
            <div class="nav-item" @click="router.push('/identity/user')">
              <TeamOutlined class="nav-icon" />
              <span>用户中心</span>
            </div>
            <div class="nav-item" @click="router.push('/organization/dept')">
              <ApartmentOutlined class="nav-icon" />
              <span>部门架构</span>
            </div>
            <div class="nav-item" @click="router.push('/authorization/role')">
              <SafetyCertificateOutlined class="nav-icon" />
              <span>角色授权</span>
            </div>
            <div class="nav-item" @click="router.push('/authorization/menu')">
              <MenuOutlined class="nav-icon" />
              <span>菜单管理</span>
            </div>
            <div class="nav-item" @click="router.push('/tenant/list')">
              <CloudServerOutlined class="nav-icon" />
              <span>租户配置</span>
            </div>
            <div class="nav-item" @click="router.push('/audit/log')">
              <HistoryOutlined class="nav-icon" />
              <span>审计日志</span>
            </div>
          </div>
        </a-card>
      </a-col>
    </a-row>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import * as echarts from 'echarts'
import {
  TeamOutlined,
  ApartmentOutlined,
  SafetyCertificateOutlined,
  CloudServerOutlined,
  MenuOutlined,
  HistoryOutlined
} from '@ant-design/icons-vue'
import { useUserStore } from '@/stores/user'
import { useTenantStore } from '@/stores/tenant'
import { useAppStore } from '@/stores/app'
import { getDashboardStats, getVisitTrend, getAuditDistribution } from '@/api/dashboard'

const router = useRouter()
const userStore = useUserStore()
const tenantStore = useTenantStore()
const appStore = useAppStore()

const stats = ref({
  userCount: 0,
  deptCount: 0,
  roleCount: 0,
  tenantCount: 0
})

const visitChartRef = ref(null)
const pieChartRef = ref(null)

let visitChart = null
let pieChart = null

const initCharts = async () => {
  if (visitChartRef.value) {
    if (visitChart) {
      visitChart.dispose()
    }
    visitChart = echarts.init(visitChartRef.value)
    const trendData = await getVisitTrend()
    
    visitChart.setOption({
      tooltip: {
        trigger: 'axis'
      },
      legend: {
        data: ['页面浏览量 (PV)', 'API 调度请求数'],
        textStyle: { color: appStore.isDark ? 'rgba(255, 255, 255, 0.85)' : '#333' }
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        boundaryGap: false,
        data: trendData.dates,
        axisLabel: { color: appStore.isDark ? 'rgba(255, 255, 255, 0.45)' : '#666' }
      },
      yAxis: {
        type: 'value',
        axisLabel: { color: appStore.isDark ? 'rgba(255, 255, 255, 0.45)' : '#666' },
        splitLine: {
          lineStyle: {
            color: appStore.isDark ? '#303030' : '#f0f0f0'
          }
        }
      },
      series: [
        {
          name: '页面浏览量 (PV)',
          type: 'line',
          smooth: true,
          data: trendData.pv,
          itemStyle: { color: '#1677ff' },
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'rgba(22, 119, 255, 0.35)' },
              { offset: 1, color: 'rgba(22, 119, 255, 0.02)' }
            ])
          }
        },
        {
          name: 'API 调度请求数',
          type: 'line',
          smooth: true,
          data: trendData.apiCalls,
          itemStyle: { color: '#52c41a' },
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'rgba(82, 196, 26, 0.35)' },
              { offset: 1, color: 'rgba(82, 196, 26, 0.02)' }
            ])
          }
        }
      ]
    })
  }

  if (pieChartRef.value) {
    if (pieChart) {
      pieChart.dispose()
    }
    pieChart = echarts.init(pieChartRef.value)
    const distribution = await getAuditDistribution()

    pieChart.setOption({
      tooltip: {
        trigger: 'item',
        formatter: '{b}: {c} ({d}%)'
      },
      legend: {
        bottom: '0%',
        left: 'center',
        textStyle: { color: appStore.isDark ? 'rgba(255, 255, 255, 0.85)' : '#333' }
      },
      series: [
        {
          name: '操作模块分布',
          type: 'pie',
          radius: ['45%', '70%'],
          avoidLabelOverlap: false,
          itemStyle: {
            borderRadius: 8,
            borderColor: appStore.isDark ? '#141414' : '#fff',
            borderWidth: 2
          },
          label: {
            show: false,
            position: 'center'
          },
          emphasis: {
            label: {
              show: true,
              fontSize: 16,
              fontWeight: 'bold'
            }
          },
          data: distribution
        }
      ]
    })
  }
}

const handleResize = () => {
  visitChart?.resize()
  pieChart?.resize()
}

watch(
  () => appStore.isDark,
  () => {
    initCharts()
  }
)

onMounted(async () => {
  const data = await getDashboardStats()
  stats.value = data
  await initCharts()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  visitChart?.dispose()
  pieChart?.dispose()
})
</script>

<style scoped>
.dashboard-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.welcome-card {
  border-radius: 8px;
}

.welcome-content {
  display: flex;
  align-items: center;
  gap: 20px;
}

.welcome-avatar {
  background: linear-gradient(135deg, #1677ff 0%, #003eb3 100%);
  font-size: 26px;
  font-weight: 600;
}

.welcome-text h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 600;
  color: var(--text-color, #1e293b);
}

.welcome-text p {
  margin: 0;
  color: var(--text-secondary, #64748b);
  font-size: 14px;
}

.stat-card {
  border-radius: 8px;
  transition: transform 0.2s, box-shadow 0.2s;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.05);
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.stat-label {
  font-size: 14px;
  color: var(--text-secondary, #8c8c8c);
  margin-bottom: 6px;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-color, #1f2937);
  line-height: 1.2;
}

.stat-desc {
  font-size: 12px;
  color: var(--text-secondary, #8c8c8c);
  margin-top: 8px;
}

.trend-up {
  color: #52c41a;
  font-weight: 600;
}

.trend-neutral {
  color: #1677ff;
  font-weight: 600;
}

.stat-icon {
  width: 52px;
  height: 52px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
}

.icon-blue {
  background: rgba(22, 119, 255, 0.12);
  color: #1677ff;
}

.icon-purple {
  background: rgba(114, 46, 209, 0.12);
  color: #722ed1;
}

.icon-green {
  background: rgba(82, 196, 26, 0.12);
  color: #52c41a;
}

.icon-orange {
  background: rgba(250, 140, 22, 0.12);
  color: #fa8c16;
}

.chart-card {
  border-radius: 8px;
}

.chart-box {
  height: 320px;
  width: 100%;
}

.info-card {
  border-radius: 8px;
  min-height: 220px;
}

.quick-nav-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  padding: 10px 0;
}

.nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 16px;
  background: var(--hover-bg, #f8fafc);
  border: 1px solid var(--border-color, transparent);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  color: var(--text-color, #334155);
  font-weight: 500;
}

.nav-item:hover {
  background: rgba(22, 119, 255, 0.12);
  border-color: #1677ff;
  color: #1677ff;
  transform: translateY(-2px);
}

.nav-icon {
  font-size: 24px;
}
</style>

