<script setup>
  import { ref, reactive, onMounted, watch } from 'vue'
  import { ProductsApi, ImageFilesApi } from '../productsApi'
  import { CategoriesApi } from '../categoriesApi'

  const products = ref([])
  const query = ref("")

  const listError = reactive({ message: "", details: [] })
  const formError = reactive({ message: "", details: [] })

  const showCreateProduct = ref(false)
  const showCreateCategory = ref(false)

  const categories = ref([])
  const loadingCats = ref(false)

  const newCategory = reactive({ name: "" })
  const newProduct = reactive({ name: "", price: "", categoryId: "", imageFileId: null })

  const saving = ref(false)

  const images = ref([])

  const editId = ref(null)
  const editModel = ref(null)

  const previewUrl = ref("")

  function formatCurrency(value) {
    if (value == null || value === "" || isNaN(value)) return "-";
    return new Intl.NumberFormat("da-DK", {
      style: "currency",
      currency: "DKK",
      minimumFractionDigits: 2,
    }).format(value);
  }

  async function loadProducts() {
    try {
      listError.message = ""; listError.details = [];
      const ctrl = new AbortController();

      const raw = query.value
        ? await ProductsApi.search(query.value, undefined, ctrl.signal)
        : await ProductsApi.list(ctrl.signal);

      const data = Array.isArray(raw)
        ? raw.map(p => ({
          id: p.id ?? p.productId ?? 0,
          name: p.name ?? p.productName ?? "",
          price: p.price ?? 0,
          categoryId: p.categoryId ?? null,
          categoryName: p.categoryName ?? "",
          imageFileId: p.imageFileId ?? null,
          imageUrl: p.imageUrl ?? ""
        }))
        : [];

      products.value = data;
    } catch (e) {
      listError.message = e.message || "Could not fetch products";
      listError.details = e.details || [];
    }
  }

  async function loadCategories() {
    loadingCats.value = true;
    try {
      const data = await CategoriesApi.list();
      categories.value = data || [];
    } finally {
      loadingCats.value = false;
    }
  }

  async function loadImages() {
    try {
      const ctrl = new AbortController();
      const data = await ImageFilesApi.list(ctrl.signal);
      images.value = Array.isArray(data) ? data : [];
    } catch (e) {
      console.warn("Failed to load images.", e);
    }
  }

  onMounted(() => { loadCategories(); loadImages(); loadProducts(); });
  watch(query, () => { loadProducts(); });

  function openCreateProduct() {
    formError.message = ""; formError.details = [];
    showCreateCategory.value = false;
    showCreateProduct.value = true;
  }
  function openCreateCategory() {
    formError.message = ""; formError.details = [];
    showCreateProduct.value = false;
    showCreateCategory.value = true;
  }

  function cancelProduct() {
    showCreateProduct.value = false;
    Object.assign(newProduct, { name: "", price: "", categoryId: "", imageFileId: null });
    saving.value = false;
    formError.message = ""; formError.details = [];
  }
  function cancelCategory() {
    showCreateCategory.value = false;
    Object.assign(newCategory, { name: "" });
    formError.message = ""; formError.details = [];
  }

  async function submitCategory() {
    formError.message = ""; formError.details = [];
    const name = newCategory.name.trim();
    if (!name) {
      formError.message = "Category name is required.";
      formError.details = [];
      return;
    }

    try {
      saving.value = true;
      const created = await CategoriesApi.create({ name });
      await loadCategories();
      newProduct.categoryId = created?.id != null ? String(created.id) : "";
      showCreateCategory.value = false;
      showCreateProduct.value = true;
    } catch (e) {
      formError.message = e.message;
      formError.details = e.details || [];
    } finally {
      saving.value = false;
    }
  }

  async function submitProduct() {
    formError.message = ""; formError.details = [];

    const name = newProduct.name.trim();
    const priceNumber = Number(newProduct.price);

    if (!name) { formError.message = "Product name is required."; return; }
    if (Number.isNaN(priceNumber) || priceNumber < 0) { formError.message = "Price must be a non-negative number."; return; }
    if (!newProduct.categoryId) { formError.message = "Select a category."; return; }

    try {
      saving.value = true;
      await ProductsApi.create({
        name,
        price: priceNumber,
        categoryId: Number(newProduct.categoryId),
        imageFileId: newProduct.imageFileId == null ? null : Number(newProduct.imageFileId)
      });
      cancelProduct();
      await loadProducts();
    } catch (e) {
      formError.message = e.message;
      formError.details = e.details || [];
    } finally {
      saving.value = false;
    }
  }

  async function deleteProduct(id) {
    const product = products.value.find(p => p.id === id);
    const name = product ? product.name : "this product";

    if (!window.confirm(`Are you sure you want to delete ${name}?`)) return;

    listError.message = ""; listError.details = [];
    try {
      await ProductsApi.remove(id);
      loadProducts();
    } catch (e) {
      listError.message = e.message || "Delete failed";
      listError.details = e.details || [];
    }
  }

  async function handleCreateUpload(files) {
    if (!files || files.length === 0) return;
    try {
      const uploaded = await ImageFilesApi.upload(files);
      if (Array.isArray(uploaded) && uploaded.length > 0) {
        const first = uploaded[0];
        images.value = images.value.some(x => x.imageFileId === first.imageFileId)
          ? images.value
          : [...images.value, first];
        newProduct.imageFileId = first.imageFileId;
      }
    } catch (e) {
      formError.message = e.message || "Upload failed.";
      formError.details = e.details || [];
    }
  }

  function startEdit(row) {
    editId.value = row.id;
    editModel.value = {
      id: row.id,
      name: row.name,
      price: row.price,
      categoryId: row.categoryId,
      imageFileId: row.imageFileId ?? null
    };
    formError.message = ""; formError.details = [];
  }

  function cancelEdit() {
    editId.value = null;
    editModel.value = null;
  }

  async function saveEdit() {
    if (!editModel.value) return;
    try {
      saving.value = true;
      const body = {
        productId: Number(editModel.value.id),
        name: String(editModel.value.name || "").trim(),
        price: Number(editModel.value.price),
        categoryId: Number(editModel.value.categoryId),
        imageFileId: editModel.value.imageFileId == null ? null : Number(editModel.value.imageFileId)
      };
      await ProductsApi.update(editModel.value.id, body);

      products.value = products.value.map(p => {
        if (p.id !== editModel.value.id) return p;
        const image = body.imageFileId ? images.value.find(x => x.imageFileId === body.imageFileId) : null;
        return {
          ...p,
          name: body.name,
          price: body.price,
          categoryId: body.categoryId,
          imageFileId: body.imageFileId,
          imageUrl: image?.url || (body.imageFileId ? p.imageUrl : "")
        };
      });

      cancelEdit();
    } catch (e) {
      formError.message = e.message || "Update failed.";
      formError.details = e.details || [];
    } finally {
      saving.value = false;
    }
  }

  async function handleEditUpload(files) {
    if (!files || files.length === 0) return;
    try {
      const uploaded = await ImageFilesApi.upload(files);
      if (Array.isArray(uploaded) && uploaded.length > 0) {
        const first = uploaded[0];
        images.value = images.value.some(x => x.imageFileId === first.imageFileId)
          ? images.value
          : [...images.value, first];
        if (editModel.value) editModel.value.imageFileId = first.imageFileId;
      }
    } catch (e) {
      formError.message = e.message || "Upload failed.";
      formError.details = e.details || [];
    }
  }

  function showImage(url) {
    if (!url) {
      listError.message = "This product has no image.";
      listError.details = [];
      return;
    }
    previewUrl.value = url;
  }
  function closePreview() {
    previewUrl.value = "";
  }

  const formStyle = { padding: "12px", border: "1px solid #ddd", borderRadius: "8px", marginBottom: "16px" };
  const gridStyle = { display: "grid", gridTemplateColumns: "140px 1fr", gap: "8px", alignItems: "center" };
  const fieldStyle = { width: "100%", padding: "8px", boxSizing: "border-box" };
  const thStyle = { textAlign: "left", borderBottom: "1px solid #ddd" };
  const thStyleCenter = { textAlign: "center", borderBottom: "1px solid #ddd" };
  const cellTextCenter = { textAlign: "center" };
  const modalBackdropStyle = {
    position: "fixed",
    inset: 0,
    background: "rgba(0,0,0,0.6)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    zIndex: 1000
  };
  const modalContentStyle = {
    background: "#fff",
    padding: "16px",
    borderRadius: "6px",
    boxShadow: "0 6px 24px rgba(0,0,0,0.4)"
  };

  function onNewProductCategoryChange(e) {
    const val = e.target.value;
    newProduct.categoryId = val === "" ? "" : parseInt(val, 10);
  }
  function onNewProductImageChange(e) {
    const val = e.target.value;
    newProduct.imageFileId = val === "" ? null : parseInt(val, 10);
  }
  function onEditImageChange(e) {
    const val = e.target.value;
    if (editModel.value) {
      editModel.value.imageFileId = val === "" ? null : parseInt(val, 10);
    }
  }</script>

<template>
  <div :style="{ maxWidth: '980px', margin: '0 auto', padding: '24px 16px' }">
    <h1>Products</h1>

    <div :style="{ display: 'flex', gap: '8px', marginBottom: '12px' }">
      <input placeholder="Search products…"
             v-model="query"
             :style="{ flex: 1, padding: '8px' }" />
      <button @click="openCreateProduct">New product</button>
      <button @click="openCreateCategory">New category</button>
    </div>

    <div v-if="listError.message"
         role="alert"
         :style="{ background:'#3b1f1f', color:'#ffdcdc', padding:'10px', borderRadius:'8px', marginTop:'8px' }">
      <div :style="{ fontWeight: 600 }">{{ listError.message }}</div>
      <ul v-if="Array.isArray(listError.details) && listError.details.length > 0" :style="{ margin:'6px 0 0 18px' }">
        <li v-for="(d, i) in listError.details" :key="i">{{ d }}</li>
      </ul>
    </div>

    <form v-if="showCreateCategory" @submit.prevent="submitCategory" :style="formStyle">
      <h2>Create category</h2>

      <div :style="gridStyle">
        <label>Name</label>
        <input v-model="newCategory.name"
               required
               :style="fieldStyle" />
      </div>

      <div v-if="formError.message"
           role="alert"
           :style="{ background:'#3b1f1f', color:'#ffdcdc', padding:'10px', borderRadius:'8px', marginTop:'8px' }">
        <div :style="{ fontWeight: 600 }">{{ formError.message }}</div>
        <ul v-if="Array.isArray(formError.details) && formError.details.length > 0" :style="{ margin:'6px 0 0 18px' }">
          <li v-for="(d, i) in formError.details" :key="i">{{ d }}</li>
        </ul>
      </div>

      <div :style="{ display:'flex', gap:'8px', marginTop:'12px' }">
        <button type="submit" :disabled="saving">{{ saving ? "Saving…" : "Create category" }}</button>
        <button type="button" @click="cancelCategory" :disabled="saving">Cancel</button>
      </div>
    </form>

    <form v-if="showCreateProduct" @submit.prevent="submitProduct" :style="formStyle">
      <h2>Create product</h2>
      <p v-if="loadingCats">Loading categories…</p>

      <div :style="gridStyle">
        <label>Name</label>
        <input v-model="newProduct.name"
               required
               :style="fieldStyle" />

        <label>Price</label>
        <input type="number"
               step="0.01"
               v-model="newProduct.price"
               required
               :style="fieldStyle" />

        <label>Category</label>
        <select :value="newProduct.categoryId === '' ? '' : String(newProduct.categoryId)"
                @change="onNewProductCategoryChange"
                required
                :style="fieldStyle">
          <option value="">Select category…</option>
          <option v-for="c in categories" :key="c.categoryId" :value="c.categoryId">{{ c.name }}</option>
        </select>

        <label>Image</label>
        <div :style="{ display:'flex', gap:'8px', alignItems:'center' }">
          <select :value="newProduct.imageFileId == null ? '' : String(newProduct.imageFileId)"
                  @change="onNewProductImageChange"
                  :style="fieldStyle">
            <option value="">(none)</option>
            <option v-for="img in images" :key="img.imageFileId" :value="img.imageFileId">
              {{ img.fileName }}
            </option>
          </select>
          <input type="file"
                 accept=".jpg,.jpeg,.png,.gif,.bmp,.webp,.svg"
                 @change="(e) => handleCreateUpload(e.target.files)" />
        </div>
      </div>

      <div v-if="formError.message"
           role="alert"
           :style="{ background:'#3b1f1f', color:'#ffdcdc', padding:'10px', borderRadius:'8px', marginTop:'8px' }">
        <div :style="{ fontWeight: 600 }">{{ formError.message }}</div>
        <ul v-if="Array.isArray(formError.details) && formError.details.length > 0" :style="{ margin:'6px 0 0 18px' }">
          <li v-for="(d, i) in formError.details" :key="i">{{ d }}</li>
        </ul>
      </div>

      <div :style="{ display:'flex', gap:'8px', marginTop:'12px' }">
        <button type="submit" :disabled="saving">{{ saving ? "Saving…" : "Create product" }}</button>
        <button type="button" @click="cancelProduct" :disabled="saving">Cancel</button>
      </div>
    </form>

    <table :style="{ width:'100%', borderCollapse:'collapse' }">
      <thead>
        <tr>
          <th :style="thStyle">Name</th>
          <th :style="thStyleCenter">Price</th>
          <th :style="thStyleCenter">Image</th>
          <th :style="thStyleCenter">Action</th>
        </tr>
      </thead>
      <tbody>
        <template v-for="p in products" :key="p.id">
          <tr v-if="editId === p.id && editModel">
            <td>
              <input v-model="editModel.name"
                     :style="fieldStyle" />
            </td>
            <td :style="cellTextCenter">
              <input type="number"
                     step="0.01"
                     v-model="editModel.price"
                     :style="{ ...fieldStyle, maxWidth:'140px' }" />
            </td>
            <td :style="cellTextCenter">
              <div :style="{ display:'flex', gap:'8px', justifyContent:'center' }">
                <select :value="editModel.imageFileId == null ? '' : String(editModel.imageFileId)"
                        @change="onEditImageChange">
                  <option value="">(none)</option>
                  <option v-for="img in images" :key="img.imageFileId" :value="img.imageFileId">
                    {{ img.fileName }}
                  </option>
                </select>
                <input type="file"
                       accept=".jpg,.jpeg,.png,.gif,.bmp,.webp,.svg"
                       @change="(e) => handleEditUpload(e.target.files)" />
              </div>
            </td>
            <td :style="{ display:'flex', gap:'8px', paddingBottom:'6px', paddingTop:'6px', justifyContent:'center' }">
              <button @click="saveEdit" :disabled="saving">{{ saving ? "Saving…" : "Save" }}</button>
              <button @click="cancelEdit" :disabled="saving">Cancel</button>
            </td>
          </tr>

          <tr v-else>
            <td>{{ p.name }}</td>
            <td :style="cellTextCenter">{{ formatCurrency(p.price) }}</td>
            <td :style="{ ...cellTextCenter, paddingTop:'6px', paddingBottom:'6px' }">
              <img v-if="p.imageUrl" :src="p.imageUrl" :alt="p.name" :style="{ width:'48px', height:'48px', objectFit:'cover', borderRadius:'4px' }" />
              <span v-else :style="{ color:'#999' }">(none)</span>
            </td>
            <td :style="{ display:'flex', gap:'8px', paddingBottom:'6px', paddingTop:'6px', justifyContent:'center' }">
              <button @click="() => startEdit(p)">Edit</button>
              <button class="deleteBtn" @click="() => deleteProduct(p.id)">Delete</button>
              <button @click="() => showImage(p.imageUrl)">Show image</button>
            </td>
          </tr>
        </template>

        <tr v-if="products.length === 0">
          <td :colspan="4" :style="{ padding:'12px', color:'#999' }">No results</td>
        </tr>
      </tbody>
    </table>

    <div v-if="previewUrl" @click="closePreview" :style="modalBackdropStyle">
      <div :style="modalContentStyle" @click.stop>
        <div :style="{ marginBottom:'8px', textAlign:'right' }">
          <button @click="closePreview">Close</button>
        </div>
        <img :src="previewUrl" alt="Preview" :style="{ maxWidth:'80vw', maxHeight:'80vh' }" />
      </div>
    </div>
  </div>
</template>

<style scoped>
  .deleteBtn {
    width: 100%;
  }
</style>
