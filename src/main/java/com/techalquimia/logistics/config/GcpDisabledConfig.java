package com.techalquimia.logistics.config;

import com.google.cloud.spring.autoconfigure.storage.GcpStorageAutoConfiguration;
import org.springframework.boot.autoconfigure.EnableAutoConfiguration;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.context.annotation.Configuration;

/**
 * Excluye la autoconfiguración de GCP Storage cuando GCP no está habilitado.
 */
@Configuration
@ConditionalOnProperty(name = "gcp.enabled", havingValue = "false", matchIfMissing = true)
@EnableAutoConfiguration(exclude = GcpStorageAutoConfiguration.class)
public class GcpDisabledConfig {
}
