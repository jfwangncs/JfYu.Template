<script lang="ts" setup>
import type { SystemPermissionApi } from '#/api/system/permission';

import type {
  OnActionClickParams,
  VxeTableGridOptions,
} from '#/adapter/vxe-table';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { RotateCw } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getPermissionList,
  syncPermissions,
  updatePermission,
} from '#/api/system/permission';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
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
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          return await getPermissionList({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            ...formValues,
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
  } as VxeTableGridOptions<SystemPermissionApi.SystemPermission>,
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
});

function onActionClick(
  e: OnActionClickParams<SystemPermissionApi.SystemPermission>,
) {
  if (e.code === 'edit') {
    formDrawerApi.setData(e.row).open();
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

function onRefresh() {
  gridApi.query();
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
        <Button type="primary" @click="onSync">
          <RotateCw class="mr-1 size-4" />
          {{ $t('system.permission.sync') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
