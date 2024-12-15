'use strict'

export const getTableItemTemplate = (item, index) => `
    <tr>
        <th scope="row">${index}</th>
        <td>${item.name}</td>
        <td>${item.price}</td>
        <td>${item.amount}</td>
        <td>${item.sum}</td>
    </tr>
`

export const getTableHeadTemplate = () => `
    <thead>
        <tr class="table-dark">
            <th scope="col">#</th>
            <th scope="col">Наименование</th>
            <th scope="col">Цена</th>
            <th scope="col">Количество</th>
            <th scope="col">Сумма</th>
        </tr>
    </thead>
`

export const getTotalLineTemplate = totalSum => `
    <thead>
        <tr class="info">
            <th scope="row" colspan="4" class="text-end">Итого:</th>            
            <td>${totalSum}</td>
        </tr>
    </thead>
`