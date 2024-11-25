'use strict'

const loadProducts = async () => {    
    const productsResponse = await fetch('/Catalog/products')    
    if (productsResponse.status !== 200)
        return []
    
    return await productsResponse.json()
}

const loadServices = async () => {
    const servicesResponse = await fetch('/Catalog/services', {
        headers: {
            'Authorization': '123'
        }
    })
    if (servicesResponse.status !== 200)
        return []

    return await servicesResponse.json()
}

const loadCatalog = async () => {
    await loadList('#products-list', loadProducts, getProductTemplate, 'Товары не найдены')
    await loadList('#services-list', loadServices, getServicesTemplate, 'Услуги не найдены')
}

const loadList = async (containerSelector, loadFunc, templateFunc, errorMessage) => {
    const items = await loadFunc()
    const listContainer = document.querySelector(containerSelector)
    if (items.length === 0) {
        listContainer.innerHTML = errorMessage
        return
    }
    for (const item of items) {
        const element = document.createElement('div')
        element.innerHTML = templateFunc(item)
        element.addEventListener('click', () => {
            alert(JSON.stringify(item))
        })
        listContainer.appendChild(element)
    }
}

const getProductTemplate = product => `
    <div class="card eshop-product-card">
        <div class="card-body">${product.name}</div>
        <div class="card-footer d-flex justify-content-between align-items-end">
            <span class="bi-currency-dollar">${product.price}</span>
            <span class="small text-secondary">В наличии: ${product.stock} шт.</span>
        </div>
    </div>
`

const getServicesTemplate = service => `
    <div class="card eshop-product-card">
        <div class="card-body">${service.name}</div>
        <div class="card-footer d-flex justify-content-between align-items-end">
            <span class="bi-currency-dollar">${service.price}</span>            
        </div>
    </div>
`

export { loadCatalog }