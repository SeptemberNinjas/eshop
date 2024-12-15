'use strict'

export const loadOrders = async () => {
    const response = await fetch('/Order')
    if (response.redirected) {
        window.location = response.url
        return
    }
    if (response.status !== 200)
        return []

    return await response.json()
}

export const createOrder = async () => {
    const response = await fetch('/Order', {
        method: 'post'
    })

    if (response.redirected)
        window.location = response.url
    
    window.location.reload()
}

const orderStatusMap = new Map()
orderStatusMap.set(0, 'Не оплачен')
orderStatusMap.set(1, 'Оплачен')

export const getOrderTemplate = (order, index) => `
    <div id="order-${order.id}" class="container bg-dark-subtle p-4 d-flex flex-column gap-3">
        <div class="bg-dark-subtle fs-5 fw-bold d-flex justify-content-between">                   
            <div><span>${index}.&nbsp;</span><span class="message-container">Статус - <span>${orderStatusMap.get(order.status)}</span></span></div>            
            <button type="button" class="pay-button btn btn-dark">Оплатить</button>
        </div>
        <table class="order-items-list table table-hover">
        </table>
    </div>
`