import { createApp } from 'vue'
import { createPinia } from 'pinia'
import Antd from 'ant-design-vue'
import 'ant-design-vue/dist/reset.css'
import './styles/theme.css'

import App from './App.vue'
import router from './router'
import './router/permission'
import setupPermissionDirective from './directives/permission'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(Antd)

// 注册 v-permission 自定义按钮权限指令
setupPermissionDirective(app)

app.mount('#app')

