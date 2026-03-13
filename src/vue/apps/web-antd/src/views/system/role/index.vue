<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import { Page, useVbenDrawer } from '@vben/common-ui';
import type { SystemRoleApi } from '#/api';
import { Plus } from '@vben/icons';
import type {
  OnActionClickParams,
  VxeTableGridOptions,
} from '#/adapter/vxe-table';
import { getRoleList, updateRole } from '#/api';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { Button, Modal } from 'ant-design-vue';
import { $t } from '#/locales';

dayjs.extend(utc);
import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';
import { useVbenVxeGrid } from '#/adapter/vxe-table';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid({
  gridOptions: {
    columns: useColumns(onActionClick, onStatusChange),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          const { startTime, endTime, ...rest } = formValues;
          return await getRoleList({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            ...(startTime && {
              startTime: dayjs(startTime).utc().format('YYYY-MM-DD HH:mm:ss'),
            }),
            ...(endTime && {
              endTime: dayjs(endTime).utc().format('YYYY-MM-DD HH:mm:ss'),
            }),
            ...rest,
          });
        },
      },
    },
    rowConfig: {
      keyField: 'id',
    },

    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: false,
    },
  } as VxeTableGridOptions<SystemRoleApi.SystemRole>,
  formOptions: {
    fieldMappingTime: [
      ['createdTime', ['startTime', 'endTime'], 'YYYY-MM-DD HH:mm:ss'],
    ],
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
});

function onActionClick(e: OnActionClickParams<SystemRoleApi.SystemRole>) {
  switch (e.code) {
    case 'edit': {
      onEdit(e.row);
      break;
    }
  }
}
function confirm(content: string, title: string) {
  return new Promise((reslove, reject) => {
    Modal.confirm({
      content,
      onCancel() {
        reject(new Error('已取消'));
      },
      onOk() {
        reslove(true);
      },
      title,
    });
  });
}
async function onStatusChange(
  newStatus: number,
  row: SystemRoleApi.SystemRole,
) {
  const status: Recordable<string> = {
    0: '禁用',
    1: '启用',
  };
  try {
    await confirm(
      `你要将${row.name}的状态切换为 【${status[newStatus.toString()]}】 吗？`,
      `切换状态`,
    );
    await updateRole(row.id, { status: newStatus });
    return true;
  } catch {
    return false;
  }
}

function onEdit(row: SystemRoleApi.SystemRole) {
  formDrawerApi.setData(row).open();
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}
</script>
<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid :table-title="$t('system.role.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('system.role.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
