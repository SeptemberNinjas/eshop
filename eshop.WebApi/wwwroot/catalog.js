'use strict'

const loadProducts = async () => {    
    const productsResponse = await fetch('/Catalog/products', {
        headers: {
            'Authorization': '123'
        }
    })
    if (productsResponse.status !== 200)
        return []
    
    return await productsResponse.json()
}

const loadCatalog = async () => {
    const products = await loadProducts()  
        
    const productsContainer = document.querySelector('#products-list')
    if (products.length === 0) {
        productsContainer.innerHTML = 'Товары не найдены'
        return
    }
    for (const product of products) {
        const productElement = document.createElement('div')
        productElement.innerHTML = getProductTemplate(product)
        productElement.addEventListener('click', () => {
            alert(JSON.stringify(product))
        })
        productsContainer.appendChild(productElement)
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

export { loadCatalog }