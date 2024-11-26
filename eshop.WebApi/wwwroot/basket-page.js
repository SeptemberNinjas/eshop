'use strict'
import {clearBasket, getBasketItemTemplate, loadBasket} from "./basket.js";

window.addEventListener('load', async () => {
    const basket = await loadBasket()
    document.querySelector('#customer-name').innerHTML = basket.customerName
    const itemsListContainer = document.querySelector('#basket-items-list')
    if (basket.items && basket.items.length){
        itemsListContainer.innerHTML = getTableHeadTemplate()
        for (let i = 0; i < basket.items.length; i++) {
            itemsListContainer.innerHTML += getBasketItemTemplate(basket.items[i], i+1)
        }
        itemsListContainer.innerHTML += getTotalLineTemplate(basket.totalSum)
    }
    else {
        itemsListContainer.innerHTML = 'Корзина пуста'
    }
    
    document.querySelector('#clear-basket').addEventListener('click', async () => {
        const itemsListContainer = document.querySelector('#basket-items-list')
        const clearResult = await clearBasket()
        if (clearResult)
            itemsListContainer.innerHTML = clearResult 
    })
})

const getTotalLineTemplate = totalSum => `
    <thead>
        <tr class="info">
            <th scope="row" colspan="4" class="text-end">Итого:</th>            
            <td>${totalSum}</td>
        </tr>
    </thead>
`
const getTableHeadTemplate = () => `
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