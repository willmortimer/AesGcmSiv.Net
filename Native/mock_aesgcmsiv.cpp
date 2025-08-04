#include "aesgcmsiv.h"
#include <cstring>

// Mock implementation for testing - NOT for production use
// This simulates the behavior without actual cryptography

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
    if (!key || key_len != AESGCMSIV_KEY_SIZE) {
        return AESGCMSIV_ERROR_INVALID_KEY;
    }
    if (!nonce || nonce_len != AESGCMSIV_NONCE_SIZE) {
        return AESGCMSIV_ERROR_INVALID_NONCE;
    }
    if (!plaintext || !ciphertext_out || !tag_out) {
        return AESGCMSIV_ERROR_INVALID_INPUT;
    }
    if (aad && !aad_len) {
        return AESGCMSIV_ERROR_INVALID_INPUT;
    }

    // Mock encryption: XOR with a simple pattern
    for (size_t i = 0; i < plaintext_len; i++) {
        ciphertext_out[i] = plaintext[i] ^ (key[i % 32]) ^ (nonce[i % 12]);
    }

    // Mock tag: simple hash-like pattern
    for (int i = 0; i < AESGCMSIV_TAG_SIZE; i++) {
        tag_out[i] = (key[i] + nonce[i % 12] + plaintext_len) & 0xFF;
    }

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
    if (!key || key_len != AESGCMSIV_KEY_SIZE) {
        return AESGCMSIV_ERROR_INVALID_KEY;
    }
    if (!nonce || nonce_len != AESGCMSIV_NONCE_SIZE) {
        return AESGCMSIV_ERROR_INVALID_NONCE;
    }
    if (!ciphertext || !tag || !plaintext_out) {
        return AESGCMSIV_ERROR_INVALID_INPUT;
    }
    if (aad && !aad_len) {
        return AESGCMSIV_ERROR_INVALID_INPUT;
    }

    // Mock decryption: XOR with the same pattern
    for (size_t i = 0; i < ciphertext_len; i++) {
        plaintext_out[i] = ciphertext[i] ^ (key[i % 32]) ^ (nonce[i % 12]);
    }

    // Mock tag verification: check if tag matches expected pattern
    for (int i = 0; i < AESGCMSIV_TAG_SIZE; i++) {
        uint8_t expected_tag = (key[i] + nonce[i % 12] + ciphertext_len) & 0xFF;
        if (tag[i] != expected_tag) {
            return AESGCMSIV_ERROR_DECRYPT_FAILED;
        }
    }

    return AESGCMSIV_SUCCESS;
}

} // extern "C" 