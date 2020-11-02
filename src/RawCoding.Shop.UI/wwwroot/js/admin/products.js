var url = '/api/admin/products';

var app = new Vue({
    el: '#app',
    data: () => ({
        form: null,
        loading: false,
        products: [],
        images: [],
    }),
    created() {
        return this.getProducts();
    },
    methods: {
        getProducts() {
            this.loading = true;
            return axios.get(url)
                .then(res => this.products = res.data)
                .catch(err => console.log(err))
                .finally(this.resetForm)
        },
        newProduct() {
            this.form = {
                name: "",
                description: "",
                series: "",
                stockDescription: "",
            }
        },
        editProduct(id) {
            this.loading = true;
            return axios.get(url + '/' + id)
                .then(res => this.form = res.data)
                .catch(err => console.log(err))
                .finally(() => this.loading = false);
        },
        addImage(event) {
            const file = event.target.files[0];
            this.images.push(file)
        },
        moveImageUp(index) {
            const image = this.form.images[index]
            this.images.splice(index, 1)
            this.images.splice(index - 1, 0, image)
        },
        moveImageDown(index) {
            const image = this.form.images[index]
            this.images.splice(index, 1)
            this.images.splice(index + 1, 0, image)
        },
        createProduct() {
            this.loading = true;
            const form = new FormData();

            form.append('form.name', this.form.name);
            form.append('form.description', this.form.description);
            form.append('form.series', this.form.series);
            form.append('form.stockDescription', this.form.stockDescription);

            for (let i = 0; i < this.images.length; i++) {
                form.append('form.images', this.images[i])
            }

            axios.post(url, form, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            })
                .then(() => this.getProducts())
                .catch(err => {
                    console.log(err);
                })
                .finally(this.resetForm)
        },
        updateProduct() {
            this.loading = true;

            const form = new FormData();

            form.append('form.id', this.form.id);
            form.append('form.name', this.form.name);
            form.append('form.description', this.form.description);
            form.append('form.series', this.form.series);
            form.append('form.stockDescription', this.form.stockDescription);

            for (let i = 0; i < this.images.length; i++) {
                form.append('form.images', this.images[i])
            }

            axios.put(url, form, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            })
                .then(() => this.getProducts())
                .catch(err => {
                    console.log(err);
                })
                .finally(this.resetForm)
        },
        publishProduct(id) {
            return axios.put(url + '/' + id + '/publish')
                .then(this.getProducts)
                .catch(err => console.log(err))
                .finally(this.resetForm);
        },
        archiveProduct(id) {
            return axios.put(url + '/' + id + '/archive')
                .then(this.getProducts)
                .catch(err => console.log(err))
                .finally(this.resetForm);
        },
        resetForm() {
            this.form = null
            this.images = []
            this.loading = false
        }
    },
    computed: {
        fileImages() {
            return this.images.map(x => URL.createObjectURL(x))
        }
    }
})