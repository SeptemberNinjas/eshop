'use strict'

export const loadBasket = async () => {
    const basketResponse = await fetch('/Basket')
    if (basketResponse.redirected){
        window.location = basketResponse.url
        return
    }    

    return await basketResponse.json()
}

export const clearBasket = async () => {
    const basketResponse = await fetch('/Basket', {
        method: 'delete'
    })
    
    if (basketResponse.redirected){
        window.location = basketResponse.url
        return
    }

    return basketResponse.status === 200 ? 'Корзина очищена' : undefined
}

export const addBasketLine = async (item) => {
    const basketResponse = await fetch('/Basket/line', {
        method: 'patch',
        headers: {
            'Content-Type': 'application/json'
        },        
        body: JSON.stringify({
            id: item.id,
            count: 1
        })
    })

    if (basketResponse.redirected){
        window.location = basketResponse.url        
    }
}

export const getBasketSize = async () => {
    const basketResponse = await fetch('/Basket')
    if (basketResponse.redirected || basketResponse.status !== 200){        
        return 0;
    }

    return (await basketResponse.json())?.items?.length ?? 0
}

export const getBasketItemTemplate = (item, index) => `
    <tr>
        <th scope="row">${index}</th>
        <td>${item.name}</td>
        <td>${item.price}</td>
        <td>${item.amount}</td>
        <td>${item.sum}</td>
    </tr>
`