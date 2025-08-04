#ifndef AESGCMSIV_H
#define AESGCMSIV_H

#include <stdint.h>
#include <stddef.h>

#ifdef _WIN32
#define AESGCMSIV_API __declspec(dllexport)
#else
#define AESGCMSIV_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

// Error codes
#define AESGCMSIV_SUCCESS 0
#define AESGCMSIV_ERROR_INVALID_KEY -1
#define AESGCMSIV_ERROR_INVALID_NONCE -2
#define AESGCMSIV_ERROR_INVALID_INPUT -3
#define AESGCMSIV_ERROR_INVALID_TAG -4
#define AESGCMSIV_ERROR_DECRYPT_FAILED -5
#define AESGCMSIV_ERROR_INTERNAL -6

// Constants
#define AESGCMSIV_KEY_SIZE 32  // 256-bit key
#define AESGCMSIV_NONCE_SIZE 12  // 96-bit nonce
#define AESGCMSIV_TAG_SIZE 16   // 128-bit authentication tag

/**
 * Encrypts plaintext using AES-GCM-SIV.
 * 
 * @param key Pointer to 32-byte key
 * @param key_len Length of key (must be 32)
 * @param nonce Pointer to 12-byte nonce
 * @param nonce_len Length of nonce (must be 12)
 * @param plaintext Pointer to plaintext data
 * @param plaintext_len Length of plaintext
 * @param aad Pointer to additional authenticated data (can be NULL)
 * @param aad_len Length of additional authenticated data
 * @param ciphertext_out Output buffer for ciphertext (must be at least plaintext_len bytes)
 * @param tag_out Output buffer for authentication tag (must be at least 16 bytes)
 * 
 * @return AESGCMSIV_SUCCESS on success, negative error code on failure
 */
AESGCMSIV_API int aesgcmsiv_encrypt(
    const uint8_t* key, size_t key_len,
    const uint8_t* nonce, size_t nonce_len,
    const uint8_t* plaintext, size_t plaintext_len,
    const uint8_t* aad, size_t aad_len,
    uint8_t* ciphertext_out,
    uint8_t* tag_out);

/**
 * Decrypts ciphertext using AES-GCM-SIV.
 * 
 * @param key Pointer to 32-byte key
 * @param key_len Length of key (must be 32)
 * @param nonce Pointer to 12-byte nonce
 * @param nonce_len Length of nonce (must be 12)
 * @param ciphertext Pointer to ciphertext data
 * @param ciphertext_len Length of ciphertext
 * @param aad Pointer to additional authenticated data (can be NULL)
 * @param aad_len Length of additional authenticated data
 * @param tag Pointer to 16-byte authentication tag
 * @param plaintext_out Output buffer for plaintext (must be at least ciphertext_len bytes)
 * 
 * @return AESGCMSIV_SUCCESS on success, negative error code on failure
 */
AESGCMSIV_API int aesgcmsiv_decrypt(
    const uint8_t* key, size_t key_len,
    const uint8_t* nonce, size_t nonce_len,
    const uint8_t* ciphertext, size_t ciphertext_len,
    const uint8_t* aad, size_t aad_len,
    const uint8_t* tag,
    uint8_t* plaintext_out);

#ifdef __cplusplus
}
#endif

#endif // AESGCMSIV_H
