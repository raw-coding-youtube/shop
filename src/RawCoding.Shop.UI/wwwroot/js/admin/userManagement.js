var app = new Vue({
    el: '#app',
    data: {
        email: "",
        users: [],
    },
    created() {
        return this.loadManagers()
    },
    methods: {
        loadManagers() {
            return axios.get('/api/admin/users')
                .then(res => this.users = res.data)
        },
        createUser() {
            return axios.post('/api/admin/users?email=' + this.email)
                .then(this.loadManagers)
                .finally(this.resetForm)
        },
        deleteUser(id) {
            return axios.delete('/api/admin/users/' + id)
                .then(this.loadManagers)
                .finally(this.resetForm)
        },
        resetForm() {
            this.email = ""
        }
    }
})