'use strict'
import {clearBasket, loadBasket} from "./basket.js";
import {createOrder, getOrderTemplate, loadOrders} from "./orders.js";
import {getTableHeadTemplate, getTableItemTemplate, getTotalLineTemplate} from "./templates.js";

window.addEventListener('load', async () => {
    await drawBasket()
    await drawOrders()    
})

async function drawBasket() {
    const basket = await loadBasket()
    document.querySelector('#customer-name').innerHTML = basket.customerName
    const itemsListContainer = document.querySelector('#basket-items-list')
    const clearButton = document.querySelector('#clear-basket')
    const createOrderButton = document.querySelector('#create-order')

    if (basket.items && basket.items.length) {
        itemsListContainer.innerHTML = getTableHeadTemplate()
        for (let i = 0; i < basket.items.length; i++) {
            itemsListContainer.innerHTML += getTableItemTemplate(basket.items[i], i + 1)
        }
        itemsListContainer.innerHTML += getTotalLineTemplate(basket.totalSum)
    } else {
        clearButton.setAttribute('style', 'display:none')
        createOrderButton.setAttribute('style', 'display:none')
        itemsListContainer.innerHTML = 'Корзина пуста'
    }

    clearButton.addEventListener('click', async () => {
        const clearResult = await clearBasket()
        if (!clearResult)
            return

        itemsListContainer.innerHTML = clearResult
        clearButton.setAttribute('style', 'display:none')
        createOrderButton.setAttribute('style', 'display:none')
    })

    createOrderButton.addEventListener('click', async () => {
        await createOrder()       
    })
}

async function drawOrders() {
    const orders = await loadOrders()
    const container = document.querySelector('#orders-list')
    orders?.forEach((o, i) => drawOrder(container, o , i))
}

const drawOrder = (container, order, index)  => {  
    const template = getOrderTemplate(order, index + 1)
    const div = document.createElement('div');
    div.innerHTML = template.trim();
    container.appendChild(div.firstChild)    
    const orderElement = container.querySelector(`#order-${order.id}`)
    const itemsListContainer = orderElement.querySelector('.order-items-list')
    const payButton = orderElement.querySelector('.pay-button')
    if (order.status > 0)
        payButton.setAttribute('style', 'display: none')
    
    payButton.addEventListener('click', async () => {
        const response = await fetch('Payment', {
            method: 'patch',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                orderId: order.id,
                paymentType: 2,
                amount: order.totalSum
            })
        })
        if (response.status !== 200)
            return
        
        const result = await response.json()
        
        if (result.isSuccess){
            payButton.setAttribute('style', 'display: none')
        }
        
        orderElement.querySelector('.message-container').innerHTML = result.message
    })

    if (order.items && order.items.length) {
        itemsListContainer.innerHTML = getTableHeadTemplate()
        for (let i = 0; i < order.items.length; i++) {
            itemsListContainer.innerHTML += getTableItemTemplate(order.items[i], i + 1)
        }
        itemsListContainer.innerHTML += getTotalLineTemplate(order.totalSum)
    }
}
