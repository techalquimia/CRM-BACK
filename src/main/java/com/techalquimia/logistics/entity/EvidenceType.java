package com.techalquimia.logistics.entity;

/**
 * Tipos de evidencia de viaje.
 */
public enum EvidenceType {

    WEIGHT_TICKET,   // Ticket de báscula (Requiere OCR)
    TRUCK_PHOTO,     // Foto del camión (Solo almacenamiento)
    SEAL_PHOTO,      // Foto del sello/precinto
    DELIVERY_PROOF,  // Prueba de entrega firmada
    INCIDENT_REPORT  // Reporte de incidente
}
