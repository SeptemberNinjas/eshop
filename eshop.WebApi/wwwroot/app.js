'use strict'
import {loadCatalog} from "./catalog.js"

window.addEventListener('load', async () => {
    await loadCatalog()
})