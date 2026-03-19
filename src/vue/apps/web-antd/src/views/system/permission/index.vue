<script lang="ts" setup>
import type { SystemPermissionApi } from '#/api/system/permission';

import type {
  OnActionClickParams,
  VxeTableGridOptions,
} from '#/adapter/vxe-table';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { IconifyIcon, Plus, RotateCw } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  deletePermission,
  getPermissionTreeList,
  syncPermissions,
  updatePermission,
} from '#/api/system/permission';
import { $t } from '#/locales';

import { useColumns } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid({
  gridOptions: {
    columns: useColumns(onActionClick, onStatusChange),
    height: 'auto',
    keepSource: true,
    pagerConfig: {
      enabled: false,
    },
    proxyConfig: {
      ajax: {
        query: async () => {
          return await getPermissionTreeList();
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
      zoom: true,
    },
    treeConfig: {
      parentField: 'parentId',
      rowField: 'id',
      transform: true,
    },
  } as VxeTableGridOptions<SystemPermissionApi.SystemPermission>,
});

function onActionClick(
  e: OnActionClickParams<SystemPermissionApi.SystemPermission>,
) {
  switch (e.code) {
    case 'append': {
      formDrawerApi.setData({ parentId: e.row.id }).open();
      break;
    }
    case 'edit': {
      formDrawerApi.setData(e.row).open();
      break;
    }
    case 'delete': {
      onDelete(e.row);
      break;
    }
  }
}

function confirm(content: string, title: string) {
  return new Promise((resolve, reject) => {
    Modal.confirm({
      content,
      onCancel() {
        reject(new Error('cancelled'));
      },
      onOk() {
        resolve(true);
      },
      title,
    });
  });
}

async function onStatusChange(
  newStatus: number,
  row: SystemPermissionApi.SystemPermission,
): Promise<boolean> {
  const label = newStatus === 1 ? $t('common.enabled') : $t('common.disabled');
  try {
    await confirm(
      `${$t('system.common.prefix-change')} ${row.name} ${$t('system.common.mid-change')} ${$t('system.permission.status')} ${$t('system.common.suffix-change')}【 ${label}】?`,
      $t('common.edit'),
    );
    await updatePermission(row.id, { status: newStatus });
    return true;
  } catch {
    return false;
  }
}

function onDelete(row: SystemPermissionApi.SystemPermission) {
  const hideLoading = message.loading({
    content: $t('ui.actionMessage.deleting', [row.name]),
    duration: 0,
    key: 'action_process_msg',
  });
  deletePermission(row.id)
    .then(() => {
      message.success({
        content: $t('ui.actionMessage.deleteSuccess', [row.name]),
        key: 'action_process_msg',
      });
      onRefresh();
    })
    .catch(() => {
      hideLoading();
    });
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData(null).open();
}

async function onSync() {
  await syncPermissions();
  message.success($t('system.permission.syncSuccess'));
  gridApi.query();
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid>
      <template #toolbar-tools>
        <Button class="mr-2" type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('common.create', [$t('system.permission.name')]) }}
        </Button>
        <Button type="default" @click="onSync">
          <RotateCw class="mr-1 size-4" />
          {{ $t('system.permission.sync') }}
        </Button>
      </template>
      <template #name="{ row }">
        <div class="flex items-center gap-1">
          <IconifyIcon
            v-if="row.icon"
            :icon="row.icon"
            class="size-4 shrink-0"
          />
          <span>{{ row.name }}</span>
        </div>
      </template>
    </Grid>
  </Page>
</template>
