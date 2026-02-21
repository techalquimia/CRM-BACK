package com.techalquimia.logistics.entity;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotBlank;
import lombok.*;

import java.time.Instant;
import java.util.UUID;

/**
 * Información básica de unidad de transporte (camión, vehículo, etc.).
 */
@Entity
@Table(name = "transport_units")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class TransportUnit {

    @Id
    @Column(length = 10, nullable = false, unique = true)
    private String id;

    @NotBlank
    @Column(nullable = false, unique = true, length = 20)
    private String placa;

    @Column(length = 50)
    private String marca;

    @Column(length = 50)
    private String modelo;

    @Column(length = 30)
    private String capacidad;

    @Column(name = "anio")
    private Integer anio;

    @Column(nullable = false)
    @Builder.Default
    private Boolean activo = true;

    @Column(length = 255)
    private String notas;

    @Column(name = "created_at", nullable = false, updatable = false)
    private Instant createdAt;

    @Column(name = "updated_at")
    private Instant updatedAt;

    @PrePersist
    protected void onCreate() {
        if (id == null) {
            id = UUID.randomUUID().toString().replace("-", "").substring(0, 10);
        }
        createdAt = Instant.now();
        updatedAt = Instant.now();
    }

    @PreUpdate
    protected void onUpdate() {
        updatedAt = Instant.now();
    }
}
