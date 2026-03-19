import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SystemPermissionApi } from '#/api/system/permission';

import dayjs from 'dayjs';
import timezone from 'dayjs/plugin/timezone';
import utc from 'dayjs/plugin/utc';

import { $t } from '#/locales';

dayjs.extend(utc);
dayjs.extend(timezone);

const typeMap: Record<number, string> = {
  1: 'Menu',
  2: 'Button',
};

export function useFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.permission.name'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'description',
      label: $t('system.permission.description'),
    },
    {
      component: 'Input',
      fieldName: 'icon',
      label: $t('system.permission.icon'),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0 },
      fieldName: 'sort',
      label: $t('system.permission.sort'),
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
        optionType: 'button',
      },
      defaultValue: 1,
      fieldName: 'status',
      label: $t('system.permission.status'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'searchKey',
      label: $t('system.permission.codeOrName'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
      },
      fieldName: 'status',
      label: $t('system.permission.status'),
    },
  ];
}

export function useColumns<T = SystemPermissionApi.SystemPermission>(
  onActionClick: OnActionClickFn<T>,
  onStatusChange?: (newStatus: any, row: T) => PromiseLike<boolean | undefined>,
): VxeTableGridOptions['columns'] {
  return [
    {
      field: 'id',
      title: $t('system.permission.id'),
      width: 80,
    },
    {
      field: 'code',
      title: $t('system.permission.code'),
      width: 220,
    },
    {
      field: 'name',
      title: $t('system.permission.name'),
      width: 180,
    },
    {
      field: 'type',
      formatter: ({ cellValue }) => typeMap[cellValue] ?? cellValue,
      title: $t('system.permission.type'),
      width: 100,
    },
    {
      field: 'sort',
      title: $t('system.permission.sort'),
      width: 80,
    },
    {
      field: 'icon',
      title: $t('system.permission.icon'),
      width: 120,
    },
    {
      field: 'description',
      minWidth: 100,
      title: $t('system.permission.description'),
    },
    {
      cellRender: {
        attrs: { beforeChange: onStatusChange },
        name: onStatusChange ? 'CellSwitch' : 'CellTag',
      },
      field: 'status',
      title: $t('system.permission.status'),
      width: 120,
    },
    {
      field: 'createdTime',
      formatter: ({ cellValue }) => {
        if (!cellValue) return '';
        return dayjs.utc(cellValue).local().format('YYYY-MM-DD HH:mm:ss');
      },
      title: $t('system.permission.createdTime'),
      width: 200,
    },
    {
      align: 'center',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('system.permission.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit'],
      },
      field: 'operation',
      fixed: 'right',
      title: $t('system.permission.operation'),
      width: 130,
    },
  ];
}
