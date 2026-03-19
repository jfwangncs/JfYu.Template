<script lang="ts" setup>
import type { SystemPermissionApi } from '#/api/system/permission';

import { computed, nextTick, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { useVbenForm } from '#/adapter/form';
import {
  createPermission,
  getPermissionTreeList,
  updatePermission,
} from '#/api/system/permission';
import { $t } from '#/locales';

import { useFormSchema } from '../data';

const emits = defineEmits(['success']);

const id = ref<number>();
const formData = ref<SystemPermissionApi.SystemPermission>();

const [Form, formApi] = useVbenForm({
  schema: [
    ...useFormSchema(),
    {
      component: 'ApiTreeSelect',
      componentProps: {
        api: getPermissionTreeList,
        childrenField: 'children',
        class: 'w-full',
        labelField: 'name',
        showSearch: true,
        treeDefaultExpandAll: true,
        valueField: 'id',
      },
      fieldName: 'parentId',
      label: $t('system.permission.parent'),
    },
  ],
  showDefaultActions: false,
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    drawerApi.lock();
    const request = id.value
      ? updatePermission(
          id.value,
          values as SystemPermissionApi.UpdatePermissionParams,
        )
      : createPermission(values as SystemPermissionApi.CreatePermissionParams);
    request
      .then(() => {
        emits('success');
        drawerApi.close();
      })
      .catch(() => {
        drawerApi.unlock();
      });
  },

  async onOpenChange(isOpen) {
    if (!isOpen) return;
    formApi.resetForm();
    const data = drawerApi.getData<SystemPermissionApi.SystemPermission>();
    if (data?.id) {
      // edit mode
      formData.value = data;
      id.value = data.id;
      await formApi.updateSchema([{ disabled: true, fieldName: 'code' }]);
      await nextTick();
      formApi.setValues({
        name: data.name,
        code: data.code,
        type: data.type,
        description: data.description,
        icon: data.icon,
        sort: data.sort,
        status: data.status,
        parentId: data.parentId,
      });
    } else {
      // create mode (or append with parentId only)
      id.value = undefined;
      formData.value = undefined;
      await formApi.updateSchema([{ disabled: false, fieldName: 'code' }]);
      if (data?.parentId) {
        await nextTick();
        formApi.setValues({ parentId: data.parentId });
      }
    }
  },
});

const getDrawerTitle = computed(() =>
  id.value
    ? $t('common.edit', [$t('system.permission.name')])
    : $t('common.create', [$t('system.permission.name')]),
);
</script>

<template>
  <Drawer :title="getDrawerTitle">
    <Form />
  </Drawer>
</template>
