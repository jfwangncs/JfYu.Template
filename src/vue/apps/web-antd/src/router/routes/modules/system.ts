import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'lucide:settings',
      order: 10,
      title: $t('page.system.title'),
    },
    name: 'System',
    path: '/system',
    children: [
      {
        name: 'UserManagement',
        path: '/system/user',
        component: () => import('#/views/system/user/index.vue'),
        meta: {
          icon: 'lucide:users',
          title: $t('page.system.user'),
          authority: ['user'],
        },
      },
      {
        name: 'RoleManagement',
        path: '/system/role',
        component: () => import('#/views/system/role/index.vue'),
        meta: {
          icon: 'lucide:shield-check',
          title: $t('page.system.role'),
          authority: ['role'],
        },
      },
      {
        name: 'PermissionManagement',
        path: '/system/permission',
        component: () => import('#/views/system/permission/index.vue'),
        meta: {
          icon: 'lucide:key',
          title: $t('page.system.permission'),
          authority: ['permission'],
        },
      },
    ],
  },
];

export default routes;
