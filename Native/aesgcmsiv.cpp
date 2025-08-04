#include "aesgcmsiv.h"
#include <openssl/evp.h>
#include <openssl/err.h>
#include <cstring>

// Helper function to validate parameters
static int validate_encrypt_params(
    const uint8_t* key, size_t key_len,
    const uint8_t* nonce, size_t nonce_len,
    const uint8_t* plaintext, size_t plaintext_len,
    const uint8_t* aad, size_t aad_len,
    uint8_t* ciphertext_out, uint8_t* tag_out)
{
    if (!key || key_len != AESGCMSIV_KEY_SIZE) {
        return AESGCMSIV_ERROR_INVALID_KEY;
    }
    if (!nonce || nonce_len != AESGCMSIV_NONCE_SIZE) {
        return AESGCMSIV_ERROR_INVALID_NONCE;
    }
    if (!plaintext || !ciphertext_out || !tag_out) {
        return AESGCMSIV_ERROR_INVALID_INPUT;
    }
    // Allow empty AAD (aad can be non-null but have zero length)
    return AESGCMSIV_SUCCESS;
}

static int validate_decrypt_params(
    const uint8_t* key, size_t key_len,
    const uint8_t* nonce, size_t nonce_len,
    const uint8_t* ciphertext, size_t ciphertext_len,
    const uint8_t* aad, size_t aad_len,
    const uint8_t* tag, uint8_t* plaintext_out)
{
    if (!key || key_len != AESGCMSIV_KEY_SIZE) {
        return AESGCMSIV_ERROR_INVALID_KEY;
    }
    if (!nonce || nonce_len != AESGCMSIV_NONCE_SIZE) {
        return AESGCMSIV_ERROR_INVALID_NONCE;
    }
    if (!ciphertext || !tag || !plaintext_out) {
        return AESGCMSIV_ERROR_INVALID_INPUT;
    }
    // Allow empty AAD (aad can be non-null but have zero length)
    return AESGCMSIV_SUCCESS;
}

extern "C" {

int aesgcmsiv_encrypt(
    const uint8_t* key, size_t key_len,
    const uint8_t* nonce, size_t nonce_len,
    const uint8_t* plaintext, size_t plaintext_len,
    const uint8_t* aad, size_t aad_len,
    uint8_t* ciphertext_out,
    uint8_t* tag_out)
{
    // Validate parameters
    int validation_result = validate_encrypt_params(
        key, key_len, nonce, nonce_len, plaintext, plaintext_len,
        aad, aad_len, ciphertext_out, tag_out);
    if (validation_result != AESGCMSIV_SUCCESS) {
        return validation_result;
    }

    // Initialize OpenSSL
    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    if (!ctx) {
        return AESGCMSIV_ERROR_INTERNAL;
    }

    // Initialize encryption
    EVP_CIPHER* cipher = EVP_CIPHER_fetch(nullptr, "AES-256-GCM-SIV", nullptr);
    if (!cipher) {
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_INTERNAL;
    }
    
    if (EVP_EncryptInit_ex2(ctx, cipher, key, nonce, nullptr) != 1) {
        EVP_CIPHER_free(cipher);
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_INTERNAL;
    }
    
    EVP_CIPHER_free(cipher);

    // Set AAD if provided
    if (aad && aad_len > 0) {
        int out_len;
        if (EVP_EncryptUpdate(ctx, nullptr, &out_len, aad, static_cast<int>(aad_len)) != 1) {
            EVP_CIPHER_CTX_free(ctx);
            return AESGCMSIV_ERROR_INTERNAL;
        }
    }

    // Encrypt plaintext
    int out_len;
    if (EVP_EncryptUpdate(ctx, ciphertext_out, &out_len, plaintext, static_cast<int>(plaintext_len)) != 1) {
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_INTERNAL;
    }

    // Finalize encryption and get tag
    if (EVP_EncryptFinal_ex(ctx, nullptr, &out_len) != 1) {
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_INTERNAL;
    }

    // Get the authentication tag
    if (EVP_CIPHER_CTX_ctrl(ctx, EVP_CTRL_AEAD_GET_TAG, AESGCMSIV_TAG_SIZE, tag_out) != 1) {
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_INTERNAL;
    }

    EVP_CIPHER_CTX_free(ctx);
    return AESGCMSIV_SUCCESS;
}

int aesgcmsiv_decrypt(
    const uint8_t* key, size_t key_len,
    const uint8_t* nonce, size_t nonce_len,
    const uint8_t* ciphertext, size_t ciphertext_len,
    const uint8_t* aad, size_t aad_len,
    const uint8_t* tag,
    uint8_t* plaintext_out)
{
    // Validate parameters
    int validation_result = validate_decrypt_params(
        key, key_len, nonce, nonce_len, ciphertext, ciphertext_len,
        aad, aad_len, tag, plaintext_out);
    if (validation_result != AESGCMSIV_SUCCESS) {
        return validation_result;
    }

    // Initialize OpenSSL
    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    if (!ctx) {
        return AESGCMSIV_ERROR_INTERNAL;
    }

    // Initialize decryption
    EVP_CIPHER* cipher = EVP_CIPHER_fetch(nullptr, "AES-256-GCM-SIV", nullptr);
    if (!cipher) {
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_INTERNAL;
    }
    
    if (EVP_DecryptInit_ex2(ctx, cipher, key, nonce, nullptr) != 1) {
        EVP_CIPHER_free(cipher);
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_INTERNAL;
    }
    
    EVP_CIPHER_free(cipher);

    // Set AAD if provided
    if (aad && aad_len > 0) {
        int out_len;
        if (EVP_DecryptUpdate(ctx, nullptr, &out_len, aad, static_cast<int>(aad_len)) != 1) {
            EVP_CIPHER_CTX_free(ctx);
            return AESGCMSIV_ERROR_INTERNAL;
        }
    }

    // Set the expected authentication tag
    if (EVP_CIPHER_CTX_ctrl(ctx, EVP_CTRL_AEAD_SET_TAG, AESGCMSIV_TAG_SIZE, const_cast<uint8_t*>(tag)) != 1) {
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_INVALID_TAG;
    }

    // Decrypt ciphertext
    int out_len;
    if (EVP_DecryptUpdate(ctx, plaintext_out, &out_len, ciphertext, static_cast<int>(ciphertext_len)) != 1) {
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_DECRYPT_FAILED;
    }

    // Finalize decryption and verify tag
    if (EVP_DecryptFinal_ex(ctx, nullptr, &out_len) != 1) {
        EVP_CIPHER_CTX_free(ctx);
        return AESGCMSIV_ERROR_DECRYPT_FAILED;
    }

    EVP_CIPHER_CTX_free(ctx);
    return AESGCMSIV_SUCCESS;
}

} // extern "C"
